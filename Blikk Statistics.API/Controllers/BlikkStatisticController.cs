using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Blikk_Statistics.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Blikk_Statistics.API.Controllers
{
    [ApiController]
    [Route(("api/getblikkstatistics/"))]
    public class BlikkStatisticController : ControllerBase
    {
        private static int Random(int max)
        {
            return new Random().Next(max);
        }

        private static string RandomCompanyName()
        {
            return "Company " + new Random().Next(100);
        }

        private static List<int> GenerateChartValuesFromCount(int count, int arrayLength)
        {
            var divideBy = Math.Floor((double) arrayLength / 2);
            var ints = new List<int>();
            var sum = 0;

            while (ints.Count < arrayLength)
            {
                if (sum > count)
                {
                    sum -= (int) Math.Floor((double) count / divideBy);
                }

                var add = Math.Floor((double) (Random(count) / divideBy) + 1);
                ints.Add((int) add);
                sum += (int) add;
            }

            if (sum > count)
            {
                var exceeded = sum - count;
                for (var i = 0; i < ints.Count; i++)
                {
                    if (ints[i] > exceeded)
                    {
                        ints[i] -= exceeded;
                        break;
                    }
                }
            }
            else if (sum < count)
            {
                var underceeded = count - sum;
                ints[arrayLength - 1] += underceeded;
            }

            return ints;
        }

        private static string RandomDate()
        {
            var random = new Random();
            var hours = random.Next(24);
            var minutes = random.Next(60);
            var hoursString = "";
            var minutesString = "";
            if (hours < 10)
            {
                hoursString = "0" + hours.ToString();
            }
            else
            {
                hoursString = hours.ToString();
            }

            if (minutes < 10)
            {
                minutesString = "0" + minutes.ToString();
            }
            else
            {
                minutesString = minutes.ToString();
            }

            var date = hoursString + ":" + minutesString;
            return date;
        }

        private static string RandomSupplier(ref int blikk, ref int visma)
        {
            if (blikk > 0)
            {
                blikk--;
                return "Blikk";
            }

            if (visma > 0)
            {
                visma--;
                return "Visma";
            }

            return "Error";
        }

        private static string RandomEdition(IEnumerable<string> editions, string tempSupplier)
        {
            var blikkEditions = new List<string>();
            var vismaEditions = new List<string>();

            var enumerable = editions.ToList();
            foreach (var edition in enumerable)
            {
                switch (edition.ToLower())
                {
                    case "documents":
                        blikkEditions.Add(edition);
                        break;
                    case "easy":
                        blikkEditions.Add(edition);
                        break;
                    case "business":
                        blikkEditions.Add(edition);
                        break;
                    case "pro":
                        blikkEditions.Add(edition);
                        vismaEditions.Add(edition);
                        break;
                    case "smart":
                        vismaEditions.Add(edition);
                        break;
                }
            }

            if (tempSupplier.ToLower() == "blikk")
            {
                var edition = "";
                while (edition == "")
                {
                    foreach (var t in blikkEditions.Where(t => Random(10) < 5))
                    {
                        edition = t;
                    }

                    if (edition != "")
                    {
                        return edition;
                    }
                }
            }
            else if (tempSupplier.ToLower() == "visma")
            {
                var edition = "";
                while (edition == "")
                {
                    foreach (var t in vismaEditions.Where(t => Random(10) < 5))
                    {
                        edition = t;
                    }

                    if (edition != "")
                    {
                        return edition;
                    }
                }

            }

            return null;
        }

        private static BaseModel GetDayUser(string supplier, string edition, string channel, int randomMax)
                {
                    var splittedEditions = edition.Split(",");

                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;
                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                            blikkRandom = Random(randomMax);
                            break;
                        case "visma" when channel == "own":
                            vismaCount = Random(randomMax);
                            break;
                        default:
                        {
                            if (channel == "reseller")
                            {
                                resellerCount = Random(randomMax / 4);
                            }
                            else
                            {
                                vismaCount = Random(randomMax);
                                blikkRandom = Random(randomMax);
                            }

                            break;
                        }
                    }

                    var count = blikkRandom + vismaCount + resellerCount;
                    var customerList = new List<Customer>();

                    var tempBlikk = blikkRandom;
                    var tempVisma = vismaCount;


                    for (var i = 0; i < count; i++)
                    {
                        var tempSupplier = RandomSupplier(ref tempBlikk, ref tempVisma);
                        customerList.Add(new Customer()
                        {
                            BelongsTo = supplier switch
                            {
                                "visma" => "Visma",
                                "all" => tempSupplier,
                                _ => "Blikk"
                            },
                            Edition = splittedEditions.Length > 1
                                ? RandomEdition(splittedEditions,
                                    supplier == "visma" || supplier == "blikk" ? supplier : tempSupplier)
                                : edition == "all"
                                    ? supplier switch
                                    {
                                        "visma" => Random(10) < 5 ? "Smart" : "Pro",
                                        "all" => tempSupplier switch
                                        {
                                            "Blikk" => Random(10) < 5 ? "Document" :
                                            Random(10) < 5 ? "Easy" :
                                            Random(10) < 5 ? "Business" :
                                            "Pro",
                                            "Visma" => Random(10) < 5 ? "Smart" : "Pro",
                                            _ => "Error"
                                        },
                                        "blikk" => Random(10) < 5 ? "Pro" :
                                        Random(10) < 5 ? "Business" :
                                        Random(10) < 5 ? "Easy" : "Documents",
                                        _ => "",
                                    }
                                    : char.ToUpper(edition[0]) + edition.Substring(1),
                            Date = RandomDate(),
                            IsChurn = false,
                            Name = RandomCompanyName()
                        });
                    }


                    var users = new BaseModel()
                    {
                        Count = count,
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        ResellerCount = resellerCount,
                        CustomerList = customerList
                    };
                    return users;
                }

                private static BaseModel GetLicence(string supplier, string channel, int randomMax)
                {
                    var documentsTotal = Random(randomMax);
                    var documentsStandard = Random(randomMax);
                    var documentsBasic = Random(randomMax);
                    var documentsLight = Random(randomMax);
                    var easyTotal = Random(randomMax);
                    var easyStandard = Random(randomMax);
                    var easyBasic = Random(randomMax);
                    var easyLight = Random(randomMax);
                    var businessTotal = Random(randomMax);
                    var businessStandard = Random(randomMax);
                    var businessBasic = Random(randomMax);
                    var businessLight = Random(randomMax);
                    var proTotal = Random(randomMax);
                    var proStandard = Random(randomMax);
                    var proBasic = Random(randomMax);
                    var proLight = Random(randomMax);
                    var vismaSmartTotal = Random(randomMax);
                    var vismaSmartStandard = Random(randomMax);
                    var vismaSmartBasic = Random(randomMax);
                    var vismaSmartLight = Random(randomMax);
                    var vismaProTotal = Random(randomMax);
                    var vismaProStandard = Random(randomMax);
                    var vismaProBasic = Random(randomMax);
                    var vismaProLight = Random(randomMax);

                    var blikkLicenceCount = 0;
                    var vismaLicenceCount = 0;

                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                            blikkLicenceCount = documentsBasic + documentsLight + documentsStandard + documentsTotal +
                                                easyBasic +
                                                easyLight + easyStandard + easyTotal + businessBasic + businessLight +
                                                businessStandard + businessTotal + proBasic + proLight + proStandard +
                                                proTotal;
                            break;
                        case "visma" when channel == "own":
                            vismaLicenceCount = vismaProBasic + vismaProLight + vismaProStandard + vismaProTotal +
                                                vismaSmartBasic +
                                                vismaSmartLight + vismaSmartStandard + vismaSmartTotal;
                            break;
                        default:
                            vismaLicenceCount = vismaProBasic + vismaProLight + vismaProStandard + vismaProTotal +
                                                vismaSmartBasic +
                                                vismaSmartLight + vismaSmartStandard + vismaSmartTotal;
                            blikkLicenceCount = documentsBasic + documentsLight + documentsStandard + documentsTotal +
                                                easyBasic +
                                                easyLight + easyStandard + easyTotal + businessBasic + businessLight +
                                                businessStandard + businessTotal + proBasic + proLight + proStandard +
                                                proTotal;
                            break;
                    }

                    var licenceCount = blikkLicenceCount + vismaLicenceCount;

                    var licences = new BaseModel()
                    {
                        Count = licenceCount,
                        BlikkCount = blikkLicenceCount,
                        VismaCount = vismaLicenceCount,
                        DocumentsBasic = documentsBasic, DocumentsLight = documentsLight,
                        DocumentsStandard = documentsStandard,
                        DocumentsTotal = documentsTotal, EasyBasic = easyBasic, EasyLight = easyLight,
                        EasyStandard = easyStandard, EasyTotal = easyTotal, BusinessBasic = businessBasic,
                        BusinessLight = businessLight, BusinessStandard = businessStandard,
                        BusinessTotal = businessTotal,
                        ProBasic = proBasic, ProLight = proLight, ProStandard = proStandard, ProTotal = proTotal,
                        VismaProBasic = vismaProBasic, VismaProLight = vismaProLight,
                        VismaProStandard = vismaProStandard,
                        VismaProTotal = vismaProTotal,
                        VismaSmartLight = vismaSmartLight, VismaSmartStandard = vismaSmartStandard,
                        VismaSmartTotal = vismaSmartTotal, VismaSmartBasic = vismaSmartBasic
                    };
                    return licences;
                }

                private static BaseModel GetDayCustomer(string supplier, string channel, string edition, int randomMax)
                {
                    var splittedEditions = edition.Split(",");
                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;
                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                            blikkRandom = Random(randomMax);
                            break;
                        case "visma" when channel == "own":
                            vismaCount = Random(randomMax);
                            break;
                        default:
                        {
                            if (channel == "reseller")
                            {
                                resellerCount = Random(randomMax / 4);
                            }
                            else
                            {
                                vismaCount = Random(randomMax);
                                blikkRandom = Random(randomMax);
                            }

                            break;
                        }
                    }

                    var count = blikkRandom + vismaCount + resellerCount;
                    var customerList = new List<Customer>();

                    var tempBlikk = blikkRandom;
                    var tempVisma = vismaCount;


                    for (var i = 0; i < count; i++)
                    {
                        var tempSupplier = RandomSupplier(ref tempBlikk, ref tempVisma);
                        customerList.Add(new Customer()
                        {
                            BelongsTo = supplier switch
                            {
                                "visma" => "Visma",
                                "all" => tempSupplier,
                                _ => "Blikk"
                            },
                            Edition = splittedEditions.Length > 1
                                ? RandomEdition(splittedEditions,
                                    supplier == "visma" || supplier == "blikk" ? supplier : tempSupplier)
                                : edition == "all"
                                    ? supplier switch
                                    {
                                        "visma" => Random(10) < 5 ? "Smart" : "Pro",
                                        "all" => tempSupplier switch
                                        {
                                            "Blikk" => Random(10) < 5 ? "Document" :
                                            Random(10) < 5 ? "Easy" :
                                            Random(10) < 5 ? "Business" :
                                            "Pro",
                                            "Visma" => Random(10) < 5 ? "Smart" : "Pro",
                                            _ => "Error"
                                        },
                                        "blikk" => Random(10) < 5 ? "Pro" :
                                        Random(10) < 5 ? "Business" :
                                        Random(10) < 5 ? "Easy" : "Documents",
                                        _ => "",
                                    }
                                    : char.ToUpper(edition[0]) + edition.Substring(1),
                            Date = RandomDate(),
                            IsChurn = false,
                            Name = RandomCompanyName()
                        });
                    }

                    var customers = new BaseModel()
                    {
                        Count = count,
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        ResellerCount = resellerCount,
                        CustomerList = customerList
                    };
                    return customers;
                }

                private static BaseModel GetDaySignups(string supplier, string channel, string edition, int randomMax)
                {
                    var splittedEditions = edition.Split(",");

                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;
                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                            blikkRandom = Random(randomMax);
                            break;
                        case "visma" when channel == "own":
                            vismaCount = Random(randomMax);
                            break;
                        default:
                        {
                            if (channel == "reseller")
                            {
                                resellerCount = Random(randomMax / 4);
                            }
                            else
                            {
                                vismaCount = Random(randomMax);
                                blikkRandom = Random(randomMax);
                            }

                            break;
                        }
                    }

                    var count = blikkRandom + vismaCount + resellerCount;
                    var customerList = new List<Customer>();

                    var tempBlikk = blikkRandom;
                    var tempVisma = vismaCount;

                    for (var i = 0; i < count; i++)
                    {
                        var tempSupplier = RandomSupplier(ref tempBlikk, ref tempVisma);
                        customerList.Add(new Customer()
                        {
                            BelongsTo = supplier switch
                            {
                                "visma" => "Visma",
                                "all" => tempSupplier,
                                _ => "Blikk"
                            },
                            Edition = splittedEditions.Length > 1
                                ? RandomEdition(splittedEditions,
                                    supplier == "visma" || supplier == "blikk" ? supplier : tempSupplier)
                                : edition == "all"
                                    ? supplier switch
                                    {
                                        "visma" => Random(10) < 5 ? "Smart" : "Pro",
                                        "all" => tempSupplier switch
                                        {
                                            "Blikk" => Random(10) < 5 ? "Document" :
                                            Random(10) < 5 ? "Easy" :
                                            Random(10) < 5 ? "Business" :
                                            "Pro",
                                            "Visma" => Random(10) < 5 ? "Smart" : "Pro",
                                            _ => "Error"
                                        },
                                        "blikk" => Random(10) < 5 ? "Pro" :
                                        Random(10) < 5 ? "Business" :
                                        Random(10) < 5 ? "Easy" : "Documents",
                                        _ => "",
                                    }
                                    : char.ToUpper(edition[0]) + edition.Substring(1),
                            Date = RandomDate(),
                            IsChurn = false,
                            Name = RandomCompanyName()
                        });
                    }

                    var signups = new BaseModel()
                    {
                        Count = count,
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        ResellerCount = resellerCount,
                        CustomerList = customerList
                    };
                    return signups;
                }

                private static BaseModel GetWeekGraph(string supplier, string channel, int randomMax)
                {
                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;


                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                        {
                            blikkRandom = Random(randomMax);
                            if (blikkRandom < 10) blikkRandom += 10;
                            break;
                        }
                        case "visma" when channel == "own":
                        {
                            vismaCount = Random(randomMax);
                            if (vismaCount < 10) vismaCount += 10;
                            break;
                        }
                        default:
                        {
                            if (channel == "reseller")
                            {
                                resellerCount = Random(randomMax / 4);
                            }
                            else
                            {
                                vismaCount = Random(randomMax);
                                blikkRandom = Random(randomMax);
                            }

                            break;
                        }
                    }


                    var count = blikkRandom + vismaCount + resellerCount;

                    var ints = GenerateChartValuesFromCount(count, 7);

                    var baseModel = new BaseModel()
                    {
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        ResellerCount = resellerCount,
                        Count = count,
                        GraphStatistics = new GraphStatistics()
                        {
                            Data = new List<GraphData>()
                            {
                                new GraphData()
                                {
                                    Key = "M",
                                    Value = ints[0],
                                },
                                new GraphData()
                                {
                                    Key = "T",
                                    Value = ints[1],
                                },
                                new GraphData()
                                {
                                    Key = "O",
                                    Value = ints[2]
                                },
                                new GraphData()
                                {
                                    Key = "T",
                                    Value = ints[3]
                                },
                                new GraphData()
                                {
                                    Key = "F",
                                    Value = ints[4]
                                },
                                new GraphData()
                                {
                                    Key = "L",
                                    Value = ints[5]
                                },
                                new GraphData()
                                {
                                    Key = "S",
                                    Value = ints[6]
                                },
                            }
                        }
                    };
                    return baseModel;
                }

                private static BaseModel GetMonthGraph(string dateTo, string dateFrom, string supplier, string channel,
                    int randomMax)
                {
                    var parsedDateFrom = DateTime.Parse(dateFrom);
                    var parsedDateTo = DateTime.Parse(dateTo);
                    var myCi = new CultureInfo("sv-SE");
                    var myCal = myCi.Calendar;

                    var week1 = myCal.GetWeekOfYear(parsedDateFrom, myCi.DateTimeFormat.CalendarWeekRule,
                        myCi.DateTimeFormat.FirstDayOfWeek);
                    var week2 = myCal.GetWeekOfYear(parsedDateTo, myCi.DateTimeFormat.CalendarWeekRule,
                        myCi.DateTimeFormat.FirstDayOfWeek);

                    var weeks = new List<int> {week2};

                    for (var i = week1; i < week2; i++)
                    {
                        weeks.Add(i);
                    }

                    weeks = weeks.OrderBy(q => q).ToList();

                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;

                    switch (supplier)
                    {
                        case "blikk" when channel == "own":
                        {
                            blikkRandom = Random(randomMax);
                            if (blikkRandom < 10) blikkRandom += 10;
                            break;
                        }
                        case "visma" when channel == "own":
                        {
                            vismaCount = Random(randomMax);
                            if (vismaCount < 10) vismaCount += 10;
                            break;
                        }
                        default:
                        {
                            if (channel == "reseller")
                            {
                                resellerCount = Random(randomMax / 4);
                            }
                            else
                            {
                                vismaCount = Random(randomMax);
                                blikkRandom = Random(randomMax);
                            }

                            break;
                        }
                    }

                    var count = blikkRandom + vismaCount + resellerCount;

                    var ints = GenerateChartValuesFromCount(count, 4);

                    var model = new BaseModel()
                    {
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        Count = count,
                        ResellerCount = resellerCount,
                        GraphStatistics = new GraphStatistics()
                        {
                            Data = new List<GraphData>()
                            {
                                new GraphData() {Key = "v." + weeks[0], Value = ints[0]},
                                new GraphData() {Key = "v." + weeks[1], Value = ints[1]},
                                new GraphData() {Key = "v." + weeks[2], Value = ints[2]},
                                new GraphData() {Key = "v." + weeks[3], Value = ints[3]},
                            }
                        }
                    };
                    return model;
                }

                private static BaseModel GetYearGraph(string supplier, string channel, int randomMax)
                {
                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;


                    if (supplier == "blikk" && channel == "own")
                    {
                        blikkRandom = Random(randomMax);
                        if (blikkRandom < 10) blikkRandom += 10;
                    }
                    else if (supplier == "visma" && channel == "own")
                    {
                        vismaCount = Random(randomMax);
                        if (vismaCount < 10) vismaCount += 10;
                    }
                    else if (channel == "reseller")
                    {
                        resellerCount = Random(randomMax);
                    }
                    else
                    {
                        vismaCount = Random(randomMax);
                        blikkRandom = Random(randomMax);
                    }

                    var count = blikkRandom + vismaCount + resellerCount;

                    var ints = GenerateChartValuesFromCount(count, 12);

                    var model = new BaseModel()
                    {
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        Count = count,
                        ResellerCount = resellerCount,
                        GraphStatistics = new GraphStatistics()
                        {
                            Data = new List<GraphData>()
                            {
                                new GraphData() {Key = "J", Value = ints[0]},
                                new GraphData() {Key = "F", Value = ints[1]},
                                new GraphData() {Key = "M", Value = ints[2]},
                                new GraphData() {Key = "A", Value = ints[3]},
                                new GraphData() {Key = "M", Value = ints[4]},
                                new GraphData() {Key = "J", Value = ints[5]},
                                new GraphData() {Key = "J", Value = ints[6]},
                                new GraphData() {Key = "A", Value = ints[7]},
                                new GraphData() {Key = "S", Value = ints[8]},
                                new GraphData() {Key = "O", Value = ints[9]},
                                new GraphData() {Key = "N", Value = ints[10]},
                                new GraphData() {Key = "D", Value = ints[11]},
                            }
                        }
                    };
                    return model;
                }

                private static string CheckSupplierChannelEdition(string supplier, string channel, string edition)
                {
                    if (supplier.ToLower() != "blikk" && supplier.ToLower() != "visma" && supplier.ToLower() != "all")
                        return null;
                    if (channel.ToLower() != "own" && channel.ToLower() != "reseller") return null;
                    //if (edition.ToLower() != "pro" && edition.ToLower() != "business" && edition.ToLower() != "documents" &&
                    //    edition.ToLower() != "easy" && edition.ToLower() != "smart") return null;

                    switch (supplier.ToLower())
                    {
                        case "visma" when edition.ToLower() == "business" || edition.ToLower() == "documents" ||
                                          edition.ToLower() == "easy":
                        case "blikk" when edition.ToLower() == "smart":
                            return null;
                        default:
                            return "Fine";
                    }
                }

                [HttpGet("day")]
                public IActionResult Day(string date, string supplier, string channel, string edition)
                {
                    if (date == null || supplier == null || channel == null || edition == null) return BadRequest();

                    if (CheckSupplierChannelEdition(supplier, channel, edition) == null) return BadRequest();

                    var users = GetDayUser(supplier, edition, channel, 20);
                    var licences = GetLicence(supplier, channel, 20);
                    var customers = GetDayCustomer(supplier, channel, edition, 20);
                    var signups = GetDaySignups(supplier, channel, edition, 20);


                    users.CustomerList = users.CustomerList.OrderBy(q => q.Date).ToList();
                    customers.CustomerList = customers.CustomerList.OrderBy(q => q.Date).ToList();
                    signups.CustomerList = signups.CustomerList.OrderBy(q => q.Date).ToList();


                    return Ok(new
                    {
                        Day = new
                        {
                            Supplier = supplier, Edition = edition, Channel = channel, Signups = signups,
                            Customers = customers,
                            Users = users, Licences = licences
                        }
                    });
                }


                [HttpGet("week")]
                public IActionResult Week(string dateFrom, string dateTo, string supplier, string channel,
                    string edition)
                {
                    if (dateFrom == null || dateTo == null || supplier == null || channel == null || edition == null)
                        return BadRequest();

                    if (CheckSupplierChannelEdition(supplier, channel, edition) == null) return BadRequest();

                    var signups = GetWeekGraph(supplier, channel, 50);
                    var customers = GetWeekGraph(supplier, channel, 50);
                    var users = GetWeekGraph(supplier, channel, 50);
                    var licences = GetLicence(supplier, channel, 30);

                    return Ok(new
                    {
                        Week = new
                        {
                            Supplier = supplier,
                            Edition = edition,
                            Channel = channel,
                            Signups = signups,
                            Customers = customers,
                            Users = users,
                            Licences = licences
                        }
                    });
                }

                [HttpGet("month")]
                public IActionResult Month(string dateFrom, string dateTo, string supplier, string channel,
                    string edition)
                {
                    if (dateFrom == null || dateTo == null || supplier == null || channel == null || edition == null)
                        return BadRequest();

                    if (CheckSupplierChannelEdition(supplier, channel, edition) == null) return BadRequest();

                    var signups = GetMonthGraph(dateTo, dateFrom, supplier, channel, 200);
                    var customers = GetMonthGraph(dateTo, dateFrom, supplier, channel, 200);
                    var users = GetMonthGraph(dateTo, dateFrom, supplier, channel, 200);
                    var licences = GetLicence(supplier, channel, 200);

                    return Ok(new
                    {
                        Month = new
                        {
                            Supplier = supplier, Edition = edition, Channel = channel, Signups = signups,
                            Customers = customers,
                            Users = users, Licences = licences
                        }
                    });
                }

                [HttpGet("year")]
                public IActionResult Year(string dateFrom, string dateTo, string supplier, string channel,
                    string edition)
                {
                    if (dateFrom == null || dateTo == null || supplier == null || channel == null || edition == null)
                        return BadRequest();

                    if (CheckSupplierChannelEdition(supplier, channel, edition) == null) return BadRequest();

                    var signups = GetYearGraph(supplier, channel, 2000);
                    var customers = GetYearGraph(supplier, channel, 300);
                    var users = GetYearGraph(supplier, channel, 3000);
                    var licences = GetLicence(supplier, channel, 3000);
                    return Ok(new
                    {
                        Year = new
                        {
                            Year = DateTime.Parse(dateFrom).Year,
                            Supplier = supplier,
                            Edition = edition,
                            Channel = channel,
                            Signups = signups, Customers = customers, Users = users,
                            Licences = licences
                        }
                    });
                }


                [HttpGet("total")]
                public IActionResult Total(string supplier, string channel, string edition)
                {
                    if (supplier == null || channel == null || edition == null)
                        return BadRequest();

                    if (CheckSupplierChannelEdition(supplier, channel, edition) == null) return BadRequest();

                    var blikkRandom = 0;
                    var vismaCount = 0;
                    var resellerCount = 0;

                    if (supplier == "blikk" && channel == "own")
                    {
                        blikkRandom = Random(2000);
                        if (blikkRandom < 10) blikkRandom += 10;
                    }
                    else if (supplier == "visma" && channel == "own")
                    {
                        vismaCount = Random(2000);
                        if (vismaCount < 10) vismaCount += 10;
                    }
                    else if (channel == "reseller")
                    {
                        resellerCount = Random(2000);
                    }
                    else
                    {
                        vismaCount = Random(2000);
                        blikkRandom = Random(2000);
                    }

                    var count = blikkRandom + vismaCount + resellerCount;

                    var ints = GenerateChartValuesFromCount(count, 12);

                    var model = new BaseModel()
                    {
                        BlikkCount = blikkRandom,
                        VismaCount = vismaCount,
                        Count = count,
                        ResellerCount = resellerCount,
                        GraphStatistics = new GraphStatistics()
                        {
                            Data = new List<GraphData>()
                            {
                                new GraphData() {Key = "2015", Value = ints[0]},
                                new GraphData() {Key = "2016", Value = ints[1]},
                                new GraphData() {Key = "2017", Value = ints[2]},
                                new GraphData() {Key = "2018", Value = ints[3]},
                                new GraphData() {Key = "2019", Value = ints[4]},
                                new GraphData() {Key = "2020", Value = ints[5]},
                            }
                        }
                    };
                    return Ok(new
                    {
                        Total = new
                        {
                            Supplier = supplier, Edition = edition, Channel = channel, Signups = model,
                            Customers = model,
                            Users = model
                        }
                    });
                }
            }
        }