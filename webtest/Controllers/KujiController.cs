using Amidakuji.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Amidakuji.Controllers
{
    public class KujiController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ModelState.Clear();

            // 受け取ったTempDataからデータを取得する
            var json = TempData["kuji"]?.ToString();
            if (json != null)
            {
                var model = JsonConvert.DeserializeObject<KujiModel>(json);
                return View(model);
            }

            return View(new KujiModel());
        }

        /// <summary>
        /// 選択肢番号を押下した呼ばれるアクション
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Index(int id, KujiModel model)
        {
            // 選択済の場合はやり直す
            if (model.SelectId > 0)
            {
                return View(model);
            }

            model.Title += " 結果";
            model.SelectId = id;    // 選択したくじ番号をセット

            return View(model);
        }
    }
}