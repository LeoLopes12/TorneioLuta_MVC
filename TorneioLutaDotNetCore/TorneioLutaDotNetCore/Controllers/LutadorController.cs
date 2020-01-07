using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TorneioLutaDotNetCore.Controllers
{
    public class LutadorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        void Check_Clicked(Object sender, EventArgs e)
        {

            // Calculate the subtotal and display the result in currency format.
            // Include tax if the check box is selected.
            //Message.Text = CalculateTotal(checkbox1.Checked).ToString("c");

        }

    }
}