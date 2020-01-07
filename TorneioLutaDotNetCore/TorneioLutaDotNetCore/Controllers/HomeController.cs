using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TorneioLutaDotNetCore.Models;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Collections;
using Microsoft.Extensions.Configuration;

namespace TorneioLutaDotNetCore.Controllers
{
    public class HomeController : Controller
    {
        
        string wsUrl = "";
        private static List<TorneioLutaDotNetCore.Models.Lutador> listFighters;

        List<Lutador> fighters_Octaves = new List<Lutador>(15);
        List<Lutador> fighters_Quarters = new List<Lutador>(7);

        List<Lutador> fighters_AlreadyInChamp = new List<Lutador>();

        List<Lutas> fights_Octaves = new List<Lutas>();
        List<Lutas> fights_Quarters = new List<Lutas>();
        List<Lutas> fights_Semi = new List<Lutas>();
        List<Lutas> fights_Final = new List<Lutas>();

        IConfiguration _iconfiguration;
        public HomeController(IConfiguration iconfiguration)
        {
            _iconfiguration = iconfiguration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Lutador()
        {
            WebClient WC = new WebClient
            {
                Encoding = Encoding.UTF8
            };

            wsUrl = _iconfiguration.GetSection("MySettings").GetSection("ApiURL").Value;

            var json = WC.DownloadString(wsUrl);
            listFighters = JsonConvert.DeserializeObject<List<TorneioLutaDotNetCore.Models.Lutador>>(json);
            return View(listFighters);
        }

        [HttpPost]
        public JsonResult Vencedor(string idFighters)
        {
            SetSelectedFighters(idFighters);
            DoChampionship();

            return Json(new Item { Id = 0, Name = fights_Final[0].winner.nome }); 
        }

        private class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private void SetSelectedFighters(string idFighters) {

            string[] arrIdFighters = idFighters.Split(",");

            for (int i = 0; i < arrIdFighters.Length; i++)
            {
                int idFighter = Convert.ToInt32(arrIdFighters[i]);
                Lutador fighter = GetFighterData(idFighter);
                fighters_Octaves.Add(fighter);
            }

        }

        private void DoChampionship() {

            int championshipMoment = 8;

            for (int i = 1; i <= 4; i++) {

                fighters_AlreadyInChamp.Clear();
                for (int x = 1; x <= championshipMoment; x++)
                    DoMatch(championshipMoment, x);

                championshipMoment /= 2;

            }

        }

        private void DoMatch(int championshipMoment, int x) {

            Lutador fighter1 = new Lutador();
            Lutador fighter2 = new Lutador();

            if (championshipMoment == 8 || championshipMoment == 4) {
                fighter1 = GetYoungestFighter(championshipMoment);
                fighter2 = GetYoungestFighter(championshipMoment);
            }
            else
            {
                if (x == 1) 
                    x = 0;

                //Vai chegar aqui 2 vezes (na semi-final e na final)

                //  No caso da semi-final...
                //      Luta1 = lutador contido no index 0 VS lutador contido no index 1
                //      Luta2 = lutador contido no index 2 VS lutador contido no index 3

                //  No caso da final...
                //      Luta Final = lutador contido no index 0 VS lutador contido no index 1
                
                fighter1 = fights_Quarters[x].winner;
                fighter2 = fights_Quarters[x+1].winner;
            }

            Lutador winner = GetWinner(fighter1, fighter2);

            switch (championshipMoment)
            {
                case 8: // Octaves

                    fights_Octaves.Add(new Lutas()
                        {
                            fighter1 = fighter1,
                            fighter2 = fighter2,
                            winner = winner
                        }
                    );

                    fighters_Quarters.Add(GetFighterData(winner.id));

                break;

                case 4: //Quarters

                    fights_Quarters.Add(new Lutas()
                        {
                            fighter1 = fighter1,
                            fighter2 = fighter2,
                            winner = winner
                        }
                    );

                break;

                case 2: //Semi

                    fights_Semi.Add(new Lutas()
                        {
                            fighter1 = fighter1,
                            fighter2 = fighter2,
                            winner = winner
                        }
                    );

                break;

                case 1: //Finals

                    fights_Final.Add(new Lutas()
                        {
                            fighter1 = fighter1,
                            fighter2 = fighter2,
                            winner = winner
                        }
                    );

                break;

            }

        }

        Lutador GetWinner(Lutador fighter1, Lutador fighter2)
        {

            int percVictoryFighter1 = Convert.ToInt32(Math.Round(Convert.ToDouble(fighter1.vitorias) / Convert.ToDouble(fighter1.lutas) * 100, 2));
            int percVictoryFighter2 = Convert.ToInt32(Math.Round(Convert.ToDouble(fighter2.vitorias) / Convert.ToDouble(fighter2.lutas) * 100, 2));

            if (percVictoryFighter1 > percVictoryFighter2)
                return fighter1; 
            else
            {

                if (percVictoryFighter2 > percVictoryFighter1)
                    return fighter2; 
                else
                {

                    // Vai cair aqui sempre que houver impate...    

                    /*
                        1.  Primeiramente será comparado a quantidade de artes marciais;
                        2.  Caso a Qtd. de Artes Marciais não seja o suficiente, será comparado o número de lutas... 
                    */

                    if (fighter1.artesMarciais.Length > fighter2.artesMarciais.Length)
                        return fighter1;
                    else
                    {

                        if (fighter2.artesMarciais.Length > fighter1.artesMarciais.Length)
                            return fighter2;
                        else
                        {

                            //Aqui será comparado o número de lutas (Segundo critério de desempate)

                            if (fighter1.lutas > fighter2.lutas)
                                return fighter1;
                            else
                            {

                                if (fighter2.lutas > fighter1.lutas)
                                    return fighter2;

                            }
                        }

                    }

                }
            }

            return fighter1; // Necessita tratamento, porém, "na teoria" - NUNCA vai cair aqui...

        }
        
        private Lutador GetYoungestFighter(int championshipMoment) {
            
            int minAge = 200;

            Lutador youngestFighter = new Lutador();
            List<Lutador> lstFighters;

            if (championshipMoment==8)
                lstFighters = fighters_Octaves;
            else
                lstFighters = fighters_Quarters;

            foreach (Lutador fighter in lstFighters)
            {

                if (fighter.idade < minAge && !fighters_AlreadyInChamp.Exists(f => f.id == fighter.id))
                {
                    minAge = fighter.idade;
                    youngestFighter = fighter;
                }

            }

            fighters_AlreadyInChamp.Add(youngestFighter);
            return youngestFighter;
        }

        Lutador GetFighterData(int id)
        {
            Lutador fighter = new Lutador();
            foreach (var lutador in listFighters)
            {
                if (lutador.id == id) {
                    fighter = lutador;
                    break;
                }
            }
            return fighter;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

/*

    string path = "C:/Users/fh118/Desktop/json.txt";
    using (StreamReader sr = new StreamReader(path))
    {
        var json = sr.ReadToEnd();
        listFighters = JsonConvert.DeserializeObject<List<TorneioLutaDotNetCore.Models.Lutador>>(json);
        return View(listFighters);
    }

 */
