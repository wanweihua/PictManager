﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SO.PictManager.Common;
using SO.PictManager.DataModel;

namespace SO.PictManager.Imaging
{
    /// <summary>
    /// 画像データクラス
    /// </summary>
    public class DataImage : IImage
    {
        #region インスタンス変数

        /// <summary>
        /// 画像テーブルデータ
        /// ※メモリ圧迫回避の為、画像のバイナリデータは保持していません。
        /// </summary>
        private TblImage _image;

        /// <summary>画像のデータサイズ</summary>
        private long _dataSize;

        #endregion

        #region プロパティ

        /// <summary>
        /// 画像キー(画像ID)を取得または設定します。
        /// </summary>
        public string Key
        {
            get { return _image.ImageId.ToString(); }
            set { GetTblImage(int.Parse(value)); }
        }

        /// <summary>
        /// 画像のタイムスタンプ(更新日時)を取得または設定します。
        /// </summary>
        public DateTime Timestamp
        {
            get { return _image.UpdatedDateTime; }
            set { _image.UpdatedDateTime = value; }
        }

        /// <summary>
        /// 画像のデータサイズを取得します。
        /// </summary>
        public long DataSize
        {
            get { return _dataSize; }
        }

        /// <summary>
        /// 画像が論理削除されているかのフラグを取得または設定します。
        /// </summary>
        public bool IsDeleted
        {
            get { return _image.DeleteFlag; }
            set { _image.DeleteFlag = value; }
        }

        /// <summary>
        /// カテゴリーIDを取得または設定します。
        /// </summary>
        public int CategoryId
        {
            get { return _image.CategoryId; }
            set { _image.CategoryId = value; }
        }

        /// <summary>
        /// 画像の説明を取得または設定します。
        /// </summary>
        public string Description
        {
            get { return _image.Description; }
            set { _image.Description = value; }
        }

        /// <summary>
        /// 画像グループのIDを取得または設定します。
        /// </summary>
        public int? GroupId
        {
            get { return _image.GroupId; }
            set { _image.GroupId = value; }
        }

        /// <summary>
        /// 画像グループの表示順を取得または設定します。
        /// </summary>
        public int? GroupOrder
        {
            get { return _image.GroupOrder; }
            set { _image.GroupOrder = value; }
        }

        /// <summary>
        /// 画像のバイトデータを取得します。
        /// </summary>
        public byte[] ImageBytes
        {
            get
            {
                using (var entities = new PictManagerEntities())
                {
                    return (from i in entities.TblImages
                            where i.ImageId == _image.ImageId
                            select i.ImageData).First();
                }
            }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        /// <param name="imageId">画像ID</param>
        public DataImage(int imageId)
        {
            GetTblImage(imageId);
        }

        #endregion

        #region GetImage - 画像オブジェクトを取得

        /// <summary>
        /// 画像オブジェクトを取得します。
        /// </summary>
        /// <returns>画像オブジェクト</returns>
        public Image GetImage()
        {
            using (var entities = new PictManagerEntities())
            {
                byte[] imageData = (from i in entities.TblImages
                                    where i.ImageId == _image.ImageId
                                    select i.ImageData).First();

                var converter = new ImageConverter();
                var img = converter.ConvertFrom(imageData) as Image;

                return img;
            }
        }

        #endregion

        #region Delete - 画像データ論理削除

        /// <summary>
        /// 画像データを論理削除します。
        /// </summary>
        public void Delete()
        {
            using (var entities = new PictManagerEntities())
            {
                var image = (from i in entities.TblImages
                             where i.ImageId == _image.ImageId
                             select i).First();

                image.DeleteFlag = true;
                image.UpdatedDateTime = DateTime.Now;

                entities.SaveChanges();

                GetTblImage(_image.ImageId);
            }

            // ログ出力
            Utilities.Logger.WriteLog(GetType().FullName, MethodBase.GetCurrentMethod().Name,
                "[DELETE] 画像ID: " + _image.ImageId.ToString());
        }

        #endregion

        #region GetTblImage - 画像テーブルデータ取得

        /// <summary>
        /// 指定されたIDの画像データを画像テーブルから取得します。
        /// 但し、画像のバイナリデータは、画像データサイズのみ取得しクリアされます。
        /// </summary>
        /// <param name="imageId">画像ID</param>
        private void GetTblImage(int imageId)
        {
            using (var entities = new PictManagerEntities())
            {
                var image = (from i in entities.TblImages
                             where i.ImageId == imageId
                             select i).First();

                entities.Entry<TblImage>(image).State = EntityState.Detached;

                _image = image;

                // データサイズを取得し、バイナリデータを削除
                _dataSize = _image.ImageData.LongLength;
                _image.ImageData = null;
            }
        }

        #endregion
    }
}
