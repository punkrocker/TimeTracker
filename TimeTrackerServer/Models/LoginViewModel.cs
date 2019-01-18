using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace TimeTrackerServer.Models
{
    public class LoginViewModel : IUser<Guid>
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName
        {
            get; set;
        }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password
        {
            get; set;
        }

        public Guid Id
        {
            get
            {
                return new Guid();
            }
        }
    }
}