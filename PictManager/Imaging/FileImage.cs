﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SO.PictManager.Common;

namespace SO.PictManager.Imaging
{
    /// <summary>
    /// 画像ファイルクラス
    /// </summary>
    public class FileImage : IImage
    {
        #region インスタンス変数

        /// <summary>画像ファイル情報</summary>
        private FileInfo _fileInfo;

        /// <summary>削除フラグ</summary>
        private bool _isDeleted = false;

        #endregion

        #region プロパティ

        /// <summary>
        /// 画像キー(画像ファイルパス)を取得・設定します。
        /// </summary>
        public string Key
        {
            get { return _fileInfo.FullName; }
            set
            {
                _fileInfo = new FileInfo(value);
                _isDeleted = false;
            }
        }

        /// <summary>
        /// 画像のタイムスタンプを取得します。
        /// </summary>
        public DateTime Timestamp
        {
            get { return _fileInfo.LastWriteTime; }
        }

        /// <summary>
        /// 画像のデータサイズを取得します。
        /// </summary>
        public long DataSize
        {
            get { return _fileInfo.Length; }
        }

        /// <summary>
        /// 画像が削除されているかのフラグを取得・設定します。
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { _isDeleted = value; }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        public FileImage(string filePath)
        {
            _fileInfo = new FileInfo(filePath);
            _isDeleted = false;
        }

        #endregion

        #region GetImage - 画像オブジェクトを取得 

        /// <summary>
        /// 画像オブジェクトを取得します。
        /// </summary>
        /// <returns>画像オブジェクト</returns>
        public Image GetImage()
        {
            Image img;
            Action actRestoreFileAttr = () => { };

            // 表示対象対象が読み取り専用の場合、それを一時的に解除
            bool readOnlyFlg = false;
            if (readOnlyFlg = _fileInfo.IsReadOnly) // ←比較では無く代入
                _fileInfo.Attributes = _fileInfo.Attributes ^ FileAttributes.ReadOnly;

            // 表示対象イメージをストリームから読み込み
            using (var fs = new FileStream(_fileInfo.FullName, FileMode.Open))
            using (Image imgTemp = Image.FromStream(fs))
            {
                // GDI+汎用エラー回避の為、ストリームの受け皿のImageから新規Imageのインスタンスを作成
                img = new Bitmap(imgTemp);
            }

            // 読み取り専用を解除した場合の再設定処理
            if (readOnlyFlg)
            {
                _fileInfo.Attributes = _fileInfo.Attributes | FileAttributes.ReadOnly;
            }

            return img;
        }

        #endregion

        #region Delete - 画像ファイルを削除

        /// <summary>
        /// 画像を削除します。
        /// </summary>
        public void Delete()
        {
            // 一時退避ディレクトリが未作成の場合は作成
            string storeDir = Path.Combine(EntryPoint.TmpDirPath, Constants.STORE_DIR_NAME);
            if (!Directory.Exists(storeDir))
                Directory.CreateDirectory(storeDir);

            // ファイルの読み取り専用属性を解除
            _fileInfo.Attributes = _fileInfo.Attributes & ~FileAttributes.ReadOnly;

            // 既に同名ファイルが存在する場合は前にアンダーバーを追加
            string movePath = Path.Combine(storeDir, _fileInfo.Name);
            while (File.Exists(movePath))
            {
                movePath = Path.Combine(storeDir, Path.GetFileName(movePath).Insert(0, "_"));
            }

            // 対象ファイルを一時退避ディレクトリへ移動
            File.Move(_fileInfo.FullName, movePath);

            // 削除済ファイルリストに追記
            string delListPath = Path.Combine(storeDir, Constants.DEL_LIST_NAME);
            using (var sw = new StreamWriter(delListPath, true))
            {
                sw.WriteLine(Path.GetFileName(movePath) + Constants.DEL_LIST_SEPARATOR + _fileInfo.FullName);
            }

            // ログ出力
            Utilities.Logger.WriteLog(GetType().FullName, MethodBase.GetCurrentMethod().Name,
                "[DELETE]" + _fileInfo.FullName);

            IsDeleted = true;
        }

        #endregion
    }
}