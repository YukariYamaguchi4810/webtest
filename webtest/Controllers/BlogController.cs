using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using blog.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace webtest.Controllers
{
    public class BlogController : Controller
    {
        private readonly ILogger<BlogController> _logger;

        //jsonファイルバス
        //private const string FilePath = "/Users/yukyamaguchi/Projects/webtest/webtest/Data";
        private const string FilePath = "C:/Users/YukYamaguchi.WISEMAN.CO.JP/source/repos/webtest/webtest/Data";
        //jsonファイル名
        private const string FileName = "blog.json";

        public BlogController(ILogger<BlogController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 「初期画面」オープン時処理
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();

        }

        /// <summary>
        /// 「掲示板【TOP】」オープン時処理
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult blog(string id)
        {
            //BlogModelをNewする
            var model = new BlogModel();

            model = GetBlogModel(id);

            //idが指定されて来たとき
            if (!string.IsNullOrEmpty(id))
            {
                //更新前のテキストと投稿時間を保持する
                TempData["Text"] = TargetJson(model);
                TempData["Regymd"] = model.Regymd;
            }
            else
            {
                TempData["Text"] = string.Empty;
                TempData["Regymd"] = string.Empty;
            }

            return View(model);
        }

        /// <summary>
        ///  「管理画面」オープン時処理
        /// </summary>
        /// <param name="id">
        /// ボタンクリック時にform内から取得された_idの値
        /// </param>
        /// <param name="name">
        /// </param>
        ///  押下したボタンのasp-route-nameに指定した名称
        /// <returns>
        ///  遷移先画面
        /// </returns>

        public IActionResult list(string id, string name)
        {
            //BlogModelをNewする
            var model = new BlogModel();

            //asp-route-nameに名称が指定されて来なかったとき
            if (string.IsNullOrEmpty(name))
            {
                //BlogModelを取得する
                model = GetBlogModel(string.Empty);
                //listビューを表示する
                return View(model);
            }
            else
            {
                //asp-route-nameの名称が"update"だったら
                if (name == "update")
                {
                    //idを添付してblogビューにリダイレクトする
                    return Redirect("blog?id=" + id);
                }
                //asp-route-nameの名称が"delete"だったら
                else if (name == "delete")
                {
                    //idを添付してconfirmビューにリダイレクトする
                    return Redirect("confirm?id=" + id);
                }
                else//asp-route-nameの名称が上記以外だったら
                {
                    //blogビューにリダイレクトする
                    return RedirectToAction("blog","blog");
                }
            }
        }


        /// <summary>
        /// 「投稿」ボタン押下時処理
        /// </summary>
        /// <param name="model">
        /// この時のmodelには入力エリアの入力値が格納されている
        ///     model.Id     :_id(hidden,更新時のみ値を持つ)
        ///     model.Name   :投稿者
        ///     model.Message:内容
        ///     model.Regymd :投稿時間（更新時のみ値を持つ)
        ///     model.Upymd  :編集時間（更新時のみ値を持つ)
        ///     （一覧エリアの情報は格納されない）
        /// </param>
        /// <returns>blogへのリダイレクト</returns>
        [HttpPost]
        public ActionResult save(BlogModel model)
        {
            //// 既定のバリデーション処理は今は封印（後で使うかも）
            //if (!ModelState.IsValid)
            //    return View(model);

            //現在の年月日・時刻を取得
            string strNow = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //新規登録なのか更新登録なのかのフラグ
            string strMode = string.Empty;

            //model.Idがnullまたは空値ならば
            if (string.IsNullOrEmpty(model.Id))
            {
                //MongoDBにInsertできるようになったあかつきには空値で良いが、
                //今はjsonテキスト書き込みのため一見MongoId風のユニークになる値を入れる
                model.Id = "6279b733f9" + DateTime.Now.ToString("yyyyMMddHHmmss");
                //新規登録と判断
                strMode = "insert";
                //model.Regymdに現在の年月日・時刻をセット
                model.Regymd = strNow;
            }
            else
            {
                //更新登録と判断
                strMode = "update";
                //そうでないならばmodel.Upymdに現在の年月日・時刻をセット
                model.Regymd = TempData["Regymd"].ToString();
                model.Upymd = strNow;
            }

            //json形式の文字列を作成
            string strTJ = TargetJson(model);

            if (strMode == "insert")
            {
                //ファイル書き込み
                using (var file = new FileStream(FilePath + "/" + FileName, FileMode.Append))
                using (var writer = new StreamWriter(file, Encoding.UTF8))
                {
                    writer.Write(strTJ+ "\n \n");
                }
            }
            else if (strMode == "update")
            {
                //jsonファイルの全内容を取得
                string strAll = get_json_text();
                string? strBeforText = string.Empty;
                if (!string.IsNullOrEmpty(TempData["Text"].ToString()))
                {
                    strBeforText = TempData["Text"].ToString();
                }

                //書き換え前の文字列が保存されているときだけ
                if (!string.IsNullOrEmpty(strBeforText))
                {
                    //ファイル書き込み
                    using (var file = new FileStream(FilePath + "/" + FileName, FileMode.Truncate))
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                    {
                        //更新前文字列を更新文字列で置き換え
                        writer.Write(strAll.Replace(strBeforText,strTJ));
                    }
                }
            }

            // blogへリダイレクト→保存したjsonを読み直して表示する
            return RedirectToAction("blog", "blog");
        }


        /// <summary>
        /// 削除確認画面表示処理
        /// </summary>
        /// <param name="id">管理画面から渡されたid</param>
        /// <returns>削除確認画面表示</returns>
        [HttpGet]
        public IActionResult confirm(string id)
        {
            //BlogModelをNewする
            var model = new BlogModel();

            //BlogModelを取得する
            model = GetBlogModel(id);

            return View(model);
        }


        /// <summary>
        /// 削除確認画面ボタン操作時処理
        /// </summary>
        /// <param name="model">formから取得されたid</param>
        ///   ※ model.Nameは空値の想定だが、
        ///      実際はボタンのasp-route-nameが格納されている模様
        /// <param name="name">ボタンのasp-route-name</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult confirm(BlogModel model, string name)
        {

            //asp-route-nameの名称が"cancel"だったら
            if (name == "cancel")
            {
                //listビューにリダイレクトする
                return RedirectToAction("list", "blog");
            }
            //asp-route-nameの名称が"delete"だったら
            else if (name == "delete")
            {
                //model.Igが空値でないなら
                if (!string.IsNullOrEmpty(model.Id))
                {

                    //Idを元にBlogModelを再取得する
                    string id = model.Id;
                    model = GetBlogModel(id);

                    //jsonファイルの内容全ての取得
                    string strAll = get_json_text();

                    //削除対象の文字列取得
                    string strTJ = TargetJson(model);

                    //ファイル書き込み
                    using (var file = new FileStream(FilePath + "/" + FileName, FileMode.Truncate))
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                    {
                        //削除対象の文字列を空文字列で置換する形で削除する
                        writer.Write(strAll.Replace(strTJ, string.Empty).Replace("\n \n\n \n", "\n \n"));

                    }
                }

                //blogビューにリダイレクトする
                return RedirectToAction("blog", "blog");
            }


            // blogへリダイレクト→保存したjsonを読み直して表示する
            return RedirectToAction("blog", "blog");
        }


        /// <summary>
        ///  get_json_text()
        ///  指定したフォルダ・ファイル名のjsonファイルを読み取り
        ///  stringに展開してリターンする
        /// </summary>
        /// <returns>string</returns>
        private string get_json_text()
        {
            //リターン文字列を初期化
            string strJT = string.Empty;

            // 指定したファイルパスをルートディレクトリとして物理ファイルのプロバイダーを生成.
            using (PhysicalFileProvider provider = new PhysicalFileProvider(@FilePath))
            {
                // 指定したファイル情報を取得する
                IFileInfo fileInfo = provider.GetFileInfo(@FileName);

                // ファイルが存在するときだけデータの読み取りを行う
                
                if (fileInfo.Exists)
                {
                    //ストリームの呼び出し
                    using var stream = fileInfo.CreateReadStream();
                    //リーダーの呼び出し
                    using var reader = new StreamReader(stream);
                    
                    strJT = reader.ReadToEnd().ToString();
                }
            }

            return strJT;
        }

        /// <summary>
        ///  get_json_List()
        ///  文字列をList<ResultModel>に展開してリターンする
        /// </summary>
        /// <param name="strJT">変換元の文字列</param> 
        /// <returns>List<ResultModel></returns>
        private List<ResultModel> get_json_List(string strJT)
        {
            //リターン用配列を初期化
            var list_bj = new List<ResultModel>();

            if (!string.IsNullOrEmpty(strJT))
            {
                //jsonテキストのデシリアライズは１つのセット{}ずつ行われるので、区切り文字を使って配列化
                string[] strjson = strJT.Split("\n \n");

                for (int i = 0; i < strjson.Count(); i++)
                {
                    ////ResultModel型の変数を用意
                    //var rm = new ResultModel();

                    ////jsonテキストをデシリアライズしてResultModel型の変数にセット
                    ///   NG!：エラーは出ないがResultModel型の変数がデータを受け取れない
                    //rm = JsonSerializer.Deserialize<ResultModel>(strjson[i]);

                    //BlogJson型の変数を用意
                    var bj = new BlogJson();

                    //ファイルの先頭・末尾のカンマ対策
                    if (!string.IsNullOrEmpty(strjson[i]))
                    {
                        if (strjson[i].Substring(0,1)==",")
                        {
                            strjson[i] = strjson[i].Replace(",\n{", "{");
                        }
                        else if (strjson[i].Substring(strjson[i].Length-1, 1) == ",")
                        {
                            strjson[i] = strjson[i].Replace("}\n,","}");
                        }

                        //jsonテキストをデシリアライズしてBlogJson型の変数にセット
                        bj = JsonSerializer.Deserialize<BlogJson>(strjson[i]);

                        //BlogJson型の変数がnullでない時だけ
                        if (bj!=null)
                        {
                            //ResultModel型の変数を用意
                            var rm = new ResultModel(bj);

                            //リターン用配列にResultModel型の変数データを先頭挿入（ソート順考慮）
                            list_bj.Insert(0, rm);
                        }   
                    }
                }

            }
            return list_bj;
        }

        /// <summary>
        /// GetBlogModel　BlogModelの取得
        /// </summary>
        /// <param name="strId">更新時にターゲットとするMongoId</param>
        /// <returns></returns>
        private BlogModel GetBlogModel(string strId)
        {
            //リターン用BlogModelをNewする
            var model = new BlogModel();

            //入力枠
            model.Id = string.Empty;
            model.Name = string.Empty;
            model.Message = string.Empty;
            model.Regymd = string.Empty;
            model.Upymd = string.Empty;

            //一覧
            model.Result = new List<ResultModel>();

            //jsonファイルの読み書き時
            string strJT = get_json_text();
            model.Result = get_json_List(strJT);

            //MongoDBの読み書き時
            //get_mongo_data();

            //引数が空値でないとき
            if (!string.IsNullOrEmpty(strId))
            {
                //model.Resultをループ
                for (int i = 0; i < model.Result.Count; i++)
                {
                    //引数がmodel.Resultの_idと同じときだけmodelの各値を更新
                    if (model.Result[i].Item._id == strId)
                    {
                        model.Id = model.Result[i].Item._id;
                        model.Name = model.Result[i].Item.name;
                        model.Message = model.Result[i].Item.message;
                        model.Regymd = model.Result[i].Item.regymd;
                        model.Upymd = model.Result[i].Item.upymd;
                        break;
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// TargetJson
        /// BlogModelのResult以外のプロパティを
        /// json形式の文字列にして返す
        /// </summary>
        /// <param name="model"></param>
        /// <returns>json形式の文字列</returns>
        private string TargetJson(BlogModel model)
        {
            string strTJ = string.Empty;

            if (!string.IsNullOrEmpty(model.Id))
            { 
                //json形式の文字列を作成
                strTJ = "{\n    \"_id\" : \"" + model.Id;
                strTJ += "\",\n    \"message\" : \"" + model.Message;
                strTJ += "\",\n    \"name\" : \"" + model.Name;
                strTJ += "\",\n    \"regymd\" : \"" + model.Regymd;
                strTJ += "\",\n    \"upymd\" : \"" + model.Upymd + "\"\n}";
            }

            return strTJ;
        }

        /// <summary>
        ///  get_mongo_data()
        ///  Mongodbからデータを取得して
        ///  List<ResultModel>形式でリターンする
        /// </summary>
        /// <returns>string</returns>
        private List<ResultModel> get_mongo_data()
        {
            //リターン用配列を初期化
            var list_bj = new List<ResultModel>();

            string connectionString = "mongodb://192.168.33.42:27017";
            MongoClient client = new MongoClient(connectionString);

            IMongoDatabase db = client.GetDatabase("mydb");
            //BlogJson型の変数を用意
            IMongoCollection<BlogJson> bj = db.GetCollection<BlogJson>("blog");

            List<BlogJson> li_bj = bj.Find(a => true).ToList();

            for (int i=0;i<li_bj.Count;i++)
            {
                //ResultModel型の変数を用意
                var rm = new ResultModel(li_bj[i]);

                //リターン用配列にResultModel型の変数データを先頭挿入（ソート順考慮）
                list_bj.Insert(0, rm);
            }

            return list_bj;
        }


    }

}

