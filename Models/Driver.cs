using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BITMasterTask.Models
{
    public class Driver
    {
        public int Id { get; set; }
        public string Surname { get; set; } = string.Empty;

        // Имя
        public string Name { get; set; } = string.Empty;

        // Отчество
        public string Patronymic { get; set; } = string.Empty;

        // Удобное отображение ФИО
        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
    }
}
