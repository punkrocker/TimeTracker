using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Dto;

namespace Model.WebResult
{
    public class UserProjectResult : WebResult
    {
        public List<UserProjectsDto> Data {
            get; set;
        }
    }
}
