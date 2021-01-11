using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Club
{
    public class Council
    {
        public Dictionary<string, CouncilMember> members;
    }

    public class CouncilMember
    {
        public string Name;
        public string Role;
    }
}
