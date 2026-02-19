using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BITMasterTask.Models
{
    public class Crew
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CarId {  get; set; }
        public int DriverId { get; set; }
        public bool Active { get; set; } = true;
    }
}
