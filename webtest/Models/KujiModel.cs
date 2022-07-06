using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Amidakuji.Models
{
    /// <summary>
    /// くじモデル
    /// </summary>
    public class KujiModel
    {
        // タイトル
        [StringLength(20, ErrorMessage = "タイトルは20文字以内で入力してください")]
        public string? Title { get; set; }
        // くじの数
        [Range(1, 9, ErrorMessage = "1～9の数字を入力してください")]
        public int NumberOfKuji { get; set; }
        // くじの結果
        public List<ResultModel>? Result { get; set; }
        // 選択したくじ番号
        public int SelectId { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KujiModel() { }
    }

    /// <summary>
    /// くじの結果モデル
    /// </summary>
    public class ResultModel
    {
        // 内容
        [StringLength(5, ErrorMessage = "くじの結果は5文字以内で入力してください")]
        public string? Item { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResultModel() { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="item"></param>
        public ResultModel(string item)
        {
            Item = item;
        }
    }
}