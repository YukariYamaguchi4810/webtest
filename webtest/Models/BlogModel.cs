using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace blog.Models
{
    /// <summary>
    /// BLOGモデル
    /// </summary>
    public class BlogModel
    {
        // MongoId []は検証に使えるデータアノテーション属性
        [BsonId] //文字列扱い
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        // 投稿者名
        [StringLength(10, ErrorMessage = "投稿者名は10文字以内で入力してください")]
        public string? Name { get; set; }
        // 投稿内容
        [StringLength(120, ErrorMessage = "投稿内容は120文字以内で入力してください")]
        public string? Message { get; set; }
        // 登録日付
        public string? Regymd { get; set; }
        // 更新日付
        public string? Upymd { get; set; }
        // 投稿記録
        public List<ResultModel>? Result { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BlogModel() { }
    }


    /// <summary>
    /// 投稿記録モデル
    /// </summary>
    public class ResultModel
    {
        public BlogJson? Item { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResultModel() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item"></param>
        public ResultModel(BlogJson item)
        {
            Item = item;
        }
    }

    /// <summary>
    /// Blog用のjsonファイルの構造
    /// </summary>
    public class BlogJson
    {
        [BsonId] //文字列扱い
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        [BsonElement("name")]
        public string? name { get; set; }
        [BsonElement("message")]
        public string? message { get; set; }
        [BsonElement("regymd")]
        public string? regymd { get; set; }
        [BsonElement("upymd")]
        public string? upymd { get; set; }
    }

}


