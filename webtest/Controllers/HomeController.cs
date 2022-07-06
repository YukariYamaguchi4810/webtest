using Amidakuji.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Amidakuji.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // 新しいくじモデルをビューに渡す
            var model = new KujiModel();

            model.Title = "晩御飯決定あみだくじ";
            model.NumberOfKuji = 5;
            model.Result = new List<ResultModel>()
            {
                new ResultModel("おでん"),
                new ResultModel("湯豆腐"),
                new ResultModel("シチュー"),
                new ResultModel("ぶり大根"),
                new ResultModel("ラーメン")
            };

            return View(model);
        }

 

    /// <summary>
    /// HomeビューでPOSTした時のアクション
    /// </summary>
    /// <param name="name"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Index(string name, KujiModel model)
    {
        // 入力エラーがある時は何もしない
        if (!ModelState.IsValid)
            return View(model);

        // [結果を設定する]ボタンを押下した場合
        if (name == "set")
        {
            // くじの数が減った場合は、結果入力欄を減らす
            while (model.NumberOfKuji < model.Result.Count)
            {
                model.Result.RemoveAt(model.Result.Count - 1);
            }

            // くじの数が増えた場合は、結果入力欄を増やす
            while (model.NumberOfKuji > model.Result.Count)
            {
                model.Result.Add(new ResultModel());
            }

            return View(model);
        }

        TempData["kuji"] = JsonConvert.SerializeObject(model);

        // くじコントローラーを呼ぶ
        return RedirectToAction("Index", "Kuji");
    }

    public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}

