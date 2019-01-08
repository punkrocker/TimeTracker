using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Dto;

namespace Model.WebResult
{
    public class CustomerTeamsResult :WebResult
    {
        public List<CustomerTeamsDto> Data { get; set; }
    }
}
