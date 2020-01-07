using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TorneioLutaDotNetCore.Models
{
    public class Lutador
    {
        public int id { get; set; }
        public string nome { get; set; }
        public int idade { get; set; }
        public string[] artesMarciais { get; set; }
        public int lutas { get; set; }
        public int derrotas { get; set; }
        public int vitorias { get; set; }
        public bool isChecked { get; set; }
    }
}
