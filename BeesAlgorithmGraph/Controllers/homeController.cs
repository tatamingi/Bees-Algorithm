using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using BeesAlgorithmGraph.Models;


namespace BeesAlgorithmGraph.Controllers
{
    public class homeController : Controller
    {
        public int nScoutBees;
        public int VarSize = 2;
        public double VarMin;      
        public double VarMax;
        public int MaxIt;
        const double rdamp = 0.95;
        public static double nSelectedSite;
        public static double nEliteSite;
        public static double nSelectedSiteBee;
        public static double nEliteSiteBee;
        public static double r;
        public static Empty_bees[] Bees;
        public static double[] bestCost;
        public List<DataPoint> dataPoints;
        public static int[] parameters = new int[5];
        public static string objectiveFunc;
        public static Random rand = new Random();

        public struct Empty_bees
        {
            public double[] Position;
            public double Cost;

            public Empty_bees(double[] pos, double cost)
            {
                Position = pos;
                Cost = cost;
            }
        }

        public struct DataPoint
        {
            public double y;

            public DataPoint(double value)
            {
                y = value;
            }
        }

        public double[] unifrnd(double VarMin, double VarMax, int VarSize) //varsize not [1 5] but 5
        {
            double[] randVector = new double[VarSize];
            for (int i = 0; i < VarSize; i++)
            {
                randVector[i] = rand.NextDouble() * (VarMax - VarMin) + VarMin;
            }
            return randVector;
        }

        public Empty_bees[] sortByCost(Empty_bees[] bees)
        {
            var sortedBees = from bee in bees
                             orderby bee.Cost
                             select bee;
            return sortedBees.ToArray();
        }

        

        public double SphereFunction(double[] vector)
        {
            double s = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                s += vector[i] * vector[i];
            }
            return s;
        }

        public double SchafferFunction(double[] x)
        {
            double y = 0.5+(Math.Pow(Math.Sin(Math.Sqrt(Math.Pow(x[0], 2)+ Math.Pow(x[1], 2))),2)-0.5)/Math.Pow(1+0.001*(Math.Pow(x[0], 2) + Math.Pow(x[1], 2)),2);
            return y;
        }

        public double GriewankFunction(double[] x)
        {
            double y = 1 / 4000 * (Math.Pow(x[0] - 100, 2) + Math.Pow(x[1] - 100, 2)) - Math.Cos(x[0] - 100) * Math.Cos((x[1] - 100) * Math.Sqrt(2)) + 1;
            return y;
        }

        public double RastriginFunction(double[] x) {
            double y = x[0] * x[0] - 10 * Math.Cos(2 * x[0] * Math.PI) + 10 + x[1] * x[1] - 10 * Math.Cos(2 * x[1] * Math.PI) + 10;
            return y;
        }

        public double RosenbrockFunction(double[] x)
        {
            double y = 100*Math.Pow(x[1]-x[0]*x[0],2)+(x[0]-1)*(x[0]-1);
            return y;
        }

        public Empty_bees[] createNewSolutions(int VarSize, int nScoutBees, double VarMin, double VarMax)
        {
            Empty_bees[] bees = new Empty_bees[nScoutBees];
            for (int i = 0; i < nScoutBees; i++)
            {
                bees[i].Position = unifrnd(VarMin, VarMax, VarSize);
                switch (objectiveFunc)
                {
                    case "Функция Шаффера":
                        bees[i].Cost = SchafferFunction(bees[i].Position);
                        break;
                    case "Сферическая функция":
                        bees[i].Cost = SphereFunction(bees[i].Position);
                        break;
                    case "Функция Griewank":
                        bees[i].Cost = GriewankFunction(bees[i].Position);
                        break;
                    case "Функция Растригина":
                        bees[i].Cost = RastriginFunction(bees[i].Position);
                        break;
                    case "Функция Розенброка":
                        bees[i].Cost = RosenbrockFunction(bees[i].Position);
                        break;                    
                }             
            }
            return bees;
        }

        public double[] PerformBeeDance(double[] vector, double r)
        {
            int index = rand.Next(0, vector.Length);
            double[] result = (double[])vector.Clone();

            result[index] = vector[index] + rand.NextDouble() * r;
            return result;
        }

        public Empty_bees[] eliteSites(Empty_bees[] bees)
        {
            for (int i = 0; i < nEliteSite; i++)
            {
                Empty_bees bestnewbee = new Empty_bees();
                Empty_bees newbee = new Empty_bees();
                bestnewbee.Cost = double.MaxValue;
                for (int j = 0; j < nEliteSiteBee; j++)
                {
                    //Console.WriteLine("{0} {1} ", bees[i].Cost, String.Join(";", bees[i].Position));
                    newbee.Position = PerformBeeDance(bees[i].Position, r);
                    //Console.WriteLine("{0} {1} ", bees[i].Cost, String.Join(";", bees[i].Position));
                    switch (objectiveFunc)
                    {
                        case "Функция Шаффера":
                            newbee.Cost = SchafferFunction(newbee.Position);
                            break;
                        case "Сферическая функция":
                            newbee.Cost = SphereFunction(newbee.Position);
                            break;
                        case "Функция Griewank":
                            newbee.Cost = GriewankFunction(newbee.Position);
                            break;
                        case "Функция Растригина":
                            newbee.Cost = RastriginFunction(newbee.Position);
                            break;
                        case "Функция Розенброка":
                            newbee.Cost = RosenbrockFunction(newbee.Position);
                            break;
                    }
                  
                    if (newbee.Cost < bestnewbee.Cost)
                        bestnewbee = newbee;
                }
                if (bestnewbee.Cost < bees[i].Cost)
                {
                    bees[i] = bestnewbee;
                }
            }
            return bees;
        }

        public Empty_bees[] nonEliteSites(Empty_bees[] bees)
        {
            for (int i = Convert.ToInt32(nEliteSite); i < nSelectedSite; i++)
            {
                Empty_bees bestnewbee = new Empty_bees();
                Empty_bees newbee = new Empty_bees();
                bestnewbee.Cost = double.MaxValue;
                for (int j = 0; j < nSelectedSiteBee; j++)
                {
                    //Console.WriteLine("{0} {1} ", bees[i].Cost, String.Join(";", bees[i].Position));
                    newbee.Position = PerformBeeDance(bees[i].Position, r);
                    //Console.WriteLine("{0} {1} ", bees[i].Cost, String.Join(";", bees[i].Position));
                    switch (objectiveFunc)
                    {
                        case "Функция Шаффера":
                            newbee.Cost = SchafferFunction(newbee.Position);
                            break;
                        case "Сферическая функция":
                            newbee.Cost = SphereFunction(newbee.Position);
                            break;
                        case "Функция Griewank":
                            newbee.Cost = GriewankFunction(newbee.Position);
                            break;
                        case "Функция Растригина":
                            newbee.Cost = RastriginFunction(newbee.Position);
                            break;
                        case "Функция Розенброка":
                            newbee.Cost = RosenbrockFunction(newbee.Position);
                            break;
                    }
                 
                    if (newbee.Cost < bestnewbee.Cost)
                        bestnewbee = newbee;
                }
                if (bestnewbee.Cost < bees[i].Cost)
                {
                    bees[i] = bestnewbee;
                }
            }
            return bees;
        }

        public Empty_bees[] nonSelectedSites(Empty_bees[] bees, int VarSize, int nScoutBees, double VarMin, double VarMax)
        {
            for (int i = Convert.ToInt32(nSelectedSite); i < nScoutBees; i++)
            {
                bees[i].Position = unifrnd(VarMin, VarMax, VarSize);
                switch (objectiveFunc)
                {
                    case "Функция Шаффера":
                        bees[i].Cost = SchafferFunction(bees[i].Position);
                        break;
                    case "Сферическая функция":
                        bees[i].Cost = SphereFunction(bees[i].Position);
                        break;
                    case "Функция Griewank":
                        bees[i].Cost = GriewankFunction(bees[i].Position);
                        break;
                    case "Функция Растригина":
                        bees[i].Cost = RastriginFunction(bees[i].Position);
                        break;
                    case "Функция Розенброка":
                        bees[i].Cost = RosenbrockFunction(bees[i].Position);
                        break;
                }
                
            }
            return bees;
        }

        // GET: home
        public ActionResult Index()
        {
            ViewData["parameters"] = parameters;
            ViewData["VarSize"] = VarSize;
            return View();
        }

        [HttpGet]
        public RedirectResult Array(List<string> names)
        {
           

            for (int i = 0; i < names.Count-1; i++)
            {
                parameters[i] = Int32.Parse(names[i]);
            }
            objectiveFunc = names[names.Count-1];
            return Redirect("/home/Index");
        }

        public ContentResult JSON()
        {
            nScoutBees = parameters[0];
            MaxIt = parameters[1];
            VarMin = parameters[2];
            VarMax = parameters[3];

            

            nSelectedSite = Math.Round(0.5 * nScoutBees);
            nEliteSite = Math.Round(0.4 * nSelectedSite);
            nSelectedSiteBee = Math.Round(0.5 * nScoutBees);
            nEliteSiteBee = 2 * nSelectedSiteBee;
            r = 0.1 * (VarMax - VarMin);

            dataPoints = new List<DataPoint>();
            homeController instanceClass = new homeController();
            Empty_bees[] Bees = instanceClass.createNewSolutions(VarSize, nScoutBees, VarMin, VarMax);
            Bees = instanceClass.sortByCost(Bees);
            Empty_bees bestSol = Bees[0];
            Empty_bees[] bestCost = new Empty_bees[MaxIt];
            double[] BestCost = new double[MaxIt];
            for (int it = 0; it < MaxIt; it++)
            {
                Bees = instanceClass.eliteSites(Bees);
                Bees = instanceClass.nonEliteSites(Bees);
                Bees = instanceClass.nonSelectedSites(Bees,VarSize, nScoutBees, VarMin, VarMax);
                Bees = instanceClass.sortByCost(Bees);
                bestSol = Bees[0];
                BestCost[it] = bestSol.Cost;
                dataPoints.Add(new DataPoint(BestCost[it]));
                //Console.WriteLine(BestCost[it]);
                r = r * rdamp;
            }

            JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
        }
    }
}