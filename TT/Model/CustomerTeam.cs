using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TT.Model
{
    public class CustomerTeam
    {
        public int teamid
        {
            get;
            set;
        }

        public string teamname
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.teamname;
        }
    }
}
