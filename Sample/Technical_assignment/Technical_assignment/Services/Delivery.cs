using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Technical_assignment.Contracts;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Technical_assignment.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly List<DayOfWeek> greenWeekDays = new();

        public DeliveryService() {
            
            var greenWeekDays = new List<DayOfWeek>();
            var cron = Settings.Cron.Expression;
            if (cron == "*")
            {
                for (int i = 0; i < 7; i++)
                {
                    greenWeekDays.Add((DayOfWeek)i);
                }
            }
            else
            {
                var weekDaysPart = cron.Split(' ')[5];
                var weekDays = weekDaysPart.Split(',');
                foreach (var day in weekDays)
                {
                    if (Enum.TryParse(day, true, out DayOfWeek dayOfWeek))
                    {
                        this.greenWeekDays.Add(dayOfWeek);
                    }
                }
            }
        }

        public async Task<List<Delivery>> CheckDeliveryDates(string postalCode, List<Product> products)
        {
            var duplicates = products.GroupBy(x => x.ProductId)
                         .Where(group => group.Count() > 1).SelectMany(prod => prod);

            if (duplicates.Any())
            {
                throw new ProductsDuplicateException();
            }

            //If there is no common delivery day among products then result is empty.
            var days = products.Select(x => x.DeliveryDays).ToList();
            List<DayOfWeek> commonDaysOfWeek = days
            .Skip(1)
            .Aggregate(new HashSet<DayOfWeek>(days.First()), (h, e) => { h.IntersectWith(e); return h; })
            .ToList();

            if (!commonDaysOfWeek.Any()) { return new List<Delivery>(); }

            var dates = await CalculateDeliveryDates(products, commonDaysOfWeek);

            List<Delivery> deliveries = new List<Delivery>();
            foreach (var date in dates)
            {
                var delivery = new Delivery { Date = date, PostalCode = postalCode };

                ProcessGreenDelivery(ref delivery);
                deliveries.Add(delivery);
            }

            //The result should be sorted in priority order, with green delivery dates at the top of the list if they are
            //within the next 3 days, otherwise dates should just be sorted ascending
            var sortedTopList = deliveries.Where(x => x.IsGreenDelivery && x.Date >= DateTime.Today.Date && x.Date <= DateTime.Today.Date.AddDays(3))
                .OrderBy(x=>x.Date).ToList();

            var sortedRemainingList = deliveries.Except(sortedTopList).OrderBy(x => x.Date).ToList();

            sortedTopList.AddRange(sortedRemainingList);

            return sortedTopList;

        }

        private async Task<List<DateTime>> CalculateDeliveryDates(List<Product> products, List<DayOfWeek> acceptableWeekDays)
        {
            var startDate = DateTime.Now.Date;
            var endDate = DateTime.Now.Date.AddDays(14);
            
            //Calculate earliest delivery dates for each product based on the start delivery date
            var productDeliveryDates = new Dictionary<Guid, ConcurrentBag<DateTime>>();

            var tasks = new List<Task>();
                
            foreach (var product in products)
            {
                tasks.Add(Task.Run(() =>
                {
                    var earliestDeliveryDates = new ConcurrentBag<DateTime>();

                    var advanceDays = product.DaysInAdvance;
                    var deliveryDays = product.DeliveryDays;
                    var earliestDeliveryDate = startDate.AddDays(GetDaysInAdvance(product));

                    while (earliestDeliveryDate <= endDate)
                    {
                        if (deliveryDays.Contains(earliestDeliveryDate.DayOfWeek) && acceptableWeekDays.Contains(earliestDeliveryDate.DayOfWeek))
                        {
                            earliestDeliveryDates.Add(earliestDeliveryDate);
                        }
                        earliestDeliveryDate = earliestDeliveryDate.AddDays(1);
                    }
                    if (earliestDeliveryDates.Count > 0)
                    {
                        productDeliveryDates.Add(product.ProductId, new ConcurrentBag<DateTime>(earliestDeliveryDates));
                    }
                }));

            }

            await Task.WhenAll(tasks);

            //If even one product does not fit into the deliverable dates then nothing should be delivered.
            if (products.Count > productDeliveryDates.Count) { return new List<DateTime>(); }

            Dictionary<Guid, List<DateTime>> deliveryDates = new();
                foreach (KeyValuePair<Guid, ConcurrentBag<DateTime>> kvp in productDeliveryDates)
            {
                List<DateTime> destList = kvp.Value.ToList();
                deliveryDates.Add(kvp.Key, destList); 
            }
            ProcessTemporaryProducts(products, ref deliveryDates);

            var commonDates =
                deliveryDates.Values.SelectMany(list => list)
                .Distinct()
                .Where(date => deliveryDates.Values.All(dates => dates.Contains(date)))
                .OrderBy(date => date)
                .ToList();

            return commonDates;
        }

        private int GetDaysInAdvance(Product product)
        {
            if (product.Type == ProductType.External)
            {
                if (product.DaysInAdvance >= 5) return product.DaysInAdvance;

                return 5;
            }
            return product.DaysInAdvance;
        }

        private void ProcessTemporaryProducts(List<Product> products, ref Dictionary<Guid, List<DateTime>> productDeliveryDates)
        {
            if (!productDeliveryDates.Any()) return;

            var productIdsWithTemporaryType = products.Where(p => p.Type == ProductType.Temporary).ToList();

            if (!productIdsWithTemporaryType.Any()) return;

            foreach (var product in productIdsWithTemporaryType)
            {
                var deliveryDates = productDeliveryDates.GetValueOrDefault(product.ProductId);

                var datesToRemove = new List<DateTime>();
                //Temporary products can only be ordered within the current week (Mon-Sun)
                foreach (var date in deliveryDates)
                {
                    if (!IsInCurrentWeek(date.Date))
                    {
                        datesToRemove.Add(date);
                    }
                }

                deliveryDates.RemoveAll(x => datesToRemove.Contains(x));
            }
        }

        private static bool IsInCurrentWeek(DateTime date)
        {
            DateTime today = DateTime.Today;
            int diff = (int)today.DayOfWeek - 1;
            DateTime startOfWeek = today.AddDays(-1 * diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            return date >= startOfWeek && date <= endOfWeek;
        }

        private void ProcessGreenDelivery(ref Delivery greenDelivery)
        {
            if (greenWeekDays.Count == 0) return;

            if (greenWeekDays.Contains(greenDelivery.Date.DayOfWeek)) greenDelivery.IsGreenDelivery = true;
        }
    }
}
