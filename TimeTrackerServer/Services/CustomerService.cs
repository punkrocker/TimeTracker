using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Model;

namespace TimeTrackerServer.Services
{
    public class CustomerService
    {
        public static IList<T_SD_Customer> GetAllCustomers()
        {
            using (var db = new TimeTrackerEntities())
            {
                return db.T_SD_Customer.ToList();
            }
        }
    }
}