using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Model;
using Model.Dto;

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

        public static IList<CustomerTeamsDto> GetCustomerTeams(int customerID)
        {
            using (var db = new TimeTrackerEntities())
            {
                var customerTeams = (from teams in db.T_SD_CustomerTeam.Where(a =>
                        a.CustomerID == customerID && a.Status == SystemConst.StatusEnable)
                    join customer in db.T_SD_Customer on teams.CustomerID equals customer.CustomerID
                        into customerTeamInfos
                    from customerTeamInfo in customerTeamInfos.DefaultIfEmpty().Where(a =>
                        a.CustomerID == customerID && a.Status == SystemConst.StatusEnable)
                    select new CustomerTeamsDto
                    {
                        CustomerName = customerTeamInfo.CustomerName,
                        Contact = customerTeamInfo.Linkman,
                        TeamId = teams.TeamID,
                        TeamName = teams.TeamName
                    }).ToList();
                return customerTeams;
            }
        }

        public static IList<T_PM_Project> GetCustomerProjects(int customerID)
        {
            using (var db = new TimeTrackerEntities())
            {
                return db.T_PM_Project.Where(a => a.CustomerID == customerID && a.Status==SystemConst.StatusIssued).ToList();
            }
        }
    }
}