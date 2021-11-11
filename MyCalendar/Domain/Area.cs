using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCalendar.Domain
{

    class Area
    {
        class Claim
        {
            public int start;
            public int end;
            public int size;
            public int line;
        }

        private static Area instance;

        public static Area GetInstance()
        {
            if (instance == null)
                instance = new Area();
            return instance;
        }

        private List<Claim> claims;

        private Area()
        {
            claims = new List<Claim>();
        }

        public bool CheckClaim(int start, int end, int line)
        {
            foreach(Claim claim in this.claims)
            {
                if (claim.line != line) continue;

                if (claim.start <= start && start <= claim.start + claim.size) return true;
                if (claim.end <= end && end <= claim.end + claim.size) return true;
            }

            return false;
        }

        public void ClaimArea(int start, int end, int size, int line)
        {
            Claim c = new Claim();
            c.start = start; c.end = end; c.line = line; c.size = size;
            claims.Add(c);
        }

        public void ResetClaim()
        {
            claims = new List<Claim>();
        }
    }
}
