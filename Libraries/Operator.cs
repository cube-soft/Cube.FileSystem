﻿/* ------------------------------------------------------------------------- */
///
/// Copyright (c) 2010 CubeSoft, Inc.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
///
/* ------------------------------------------------------------------------- */
using System;

namespace Cube.FileSystem
{
    /* --------------------------------------------------------------------- */
    ///
    /// Operator
    /// 
    /// <summary>
    /// ファイル操作を実行するクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class Operator
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// Operator
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public Operator() : this(new OperatorCore()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// Operator
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        /// 
        /// <param name="core">各種操作を実行するオブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public Operator(IOperatorCore core)
        {
            _core = core;
        }

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Get
        ///
        /// <summary>
        /// ファイルまたはディレクトリの情報を保持するオブジェクトを
        /// 取得します。
        /// </summary>
        /// 
        /// <param name="path">対象となるパス</param>
        /// 
        /// <returns>IInformation オブジェクト</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public IInformation Get(string path) => _core.Get(path);

        /* ----------------------------------------------------------------- */
        ///
        /// GetFiles
        ///
        /// <summary>
        /// ディレクトリ下にあるファイルの一覧を取得します。
        /// </summary>
        /// 
        /// <param name="path">パス</param>
        /// 
        /// <returns>ファイル一覧</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public string[] GetFiles(string path) => _core.GetFiles(path);

        /* ----------------------------------------------------------------- */
        ///
        /// GetDirectories
        ///
        /// <summary>
        /// ディレクトリ下にあるディレクトリの一覧を取得します。
        /// </summary>
        /// 
        /// <param name="path">パス</param>
        /// 
        /// <returns>ディレクトリ一覧</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public string[] GetDirectories(string path) => _core.GetDirectories(path);

        /* ----------------------------------------------------------------- */
        ///
        /// Combine
        ///
        /// <summary>
        /// パスを結合します。
        /// </summary>
        /// 
        /// <param name="paths">結合するパス一覧</param>
        /// 
        /* ----------------------------------------------------------------- */
        public string Combine(params string[] paths) => _core.Combine(paths);

        /* ----------------------------------------------------------------- */
        ///
        /// Delete
        ///
        /// <summary>
        /// ファイルまたはディレクトリを削除します。
        /// </summary>
        /// 
        /// <param name="path">削除するファイルのパス</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Delete(string path)
            => Action(nameof(Delete), () => _core.Delete(path), path);

        /* ----------------------------------------------------------------- */
        ///
        /// CreateDirectory
        ///
        /// <summary>
        /// ディレクトリを作成します。
        /// </summary>
        /// 
        /// <param name="path">ディレクトリのパス</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void CreateDirectory(string path)
            => Action(nameof(CreateDirectory), () => _core.CreateDirectory(path), path);

        /* ----------------------------------------------------------------- */
        ///
        /// Create
        ///
        /// <summary>
        /// ファイルを新規作成します。
        /// </summary>
        /// 
        /// <param name="path">ファイルのパス</param>
        /// 
        /// <returns>書き込み用ストリーム</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public System.IO.FileStream Create(string path)
            => Func(nameof(Create), () => _core.Create(path), path);

        /* ----------------------------------------------------------------- */
        ///
        /// OpenRead
        ///
        /// <summary>
        /// ファイルを読み込み専用で開きます。
        /// </summary>
        /// 
        /// <param name="path">ファイルのパス</param>
        /// 
        /// <returns>読み込み用ストリーム</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public System.IO.FileStream OpenRead(string path)
            => Func(nameof(OpenRead), () => _core.OpenRead(path), path);

        /* ----------------------------------------------------------------- */
        ///
        /// OpenWrite
        ///
        /// <summary>
        /// ファイルを新規作成、または上書き用で開きます。
        /// </summary>
        /// 
        /// <param name="path">ファイルのパス</param>
        /// 
        /// <returns>書き込み用ストリーム</returns>
        /// 
        /* ----------------------------------------------------------------- */
        public System.IO.FileStream OpenWrite(string path)
            => Func(nameof(OpenWrite), () => _core.OpenWrite(path), path);

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// ファイルを移動します。
        /// </summary>
        /// 
        /// <param name="src">移動前のパス</param>
        /// <param name="dest">移動後のパス</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Move(string src, string dest) => Move(src, dest, false);

        /* ----------------------------------------------------------------- */
        ///
        /// Move
        ///
        /// <summary>
        /// ファイルを移動します。
        /// </summary>
        /// 
        /// <param name="src">移動前のパス</param>
        /// <param name="dest">移動後のパス</param>
        /// <param name="overwrite">上書きするかどうかを表す値</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Move(string src, string dest, bool overwrite)
        {
            var si = _core.Get(src);
            var di = _core.Get(dest);

            if (!overwrite || !di.Exists) MoveCore(si, di);
            else
            {
                var tmp = _core.Combine(si.DirectoryName, Guid.NewGuid().ToString("D"));
                var ti  = _core.Get(tmp);

                if (!MoveCore(di, ti)) return;
                if ( MoveCore(si, di)) _core.Delete(tmp);
                else MoveCore(ti, di); // recover
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Copy
        ///
        /// <summary>
        /// ファイルをコピーします。
        /// </summary>
        /// 
        /// <param name="src">コピー元のパス</param>
        /// <param name="dest">コピー先のパス</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Copy(string src, string dest) => Copy(src, dest, false);

        /* ----------------------------------------------------------------- */
        ///
        /// Copy
        ///
        /// <summary>
        /// ファイルをコピーします。
        /// </summary>
        /// 
        /// <param name="src">コピー元のパス</param>
        /// <param name="dest">コピー先のパス</param>
        /// <param name="overwrite">上書きするかどうかを示す値</param>
        /// 
        /* ----------------------------------------------------------------- */
        public void Copy(string src, string dest, bool overwrite)
            => Action(nameof(Copy), () => _core.Copy(src, dest, overwrite), src, dest);

        #endregion

        #region Events

        /* ----------------------------------------------------------------- */
        ///
        /// Failed
        ///
        /// <summary>
        /// 操作に失敗した時に発生するイベントです。
        /// </summary>
        /// 
        /// <remarks>
        /// Key には失敗したメソッド名、Value には失敗した時に送出された例外
        /// オブジェクトが設定されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public event FailedEventHandler Failed;

        /* ----------------------------------------------------------------- */
        ///
        /// OnFailed
        ///
        /// <summary>
        /// Failed イベントを発生させます。
        /// </summary>
        /// 
        /// <remarks>
        /// Failed イベントにハンドラが設定されていない場合、
        /// 例外をそのまま送出します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnFailed(FailedEventArgs e)
        {
            if (Failed != null) Failed(this, e);
            else throw e.Exception;
        }

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// MoveCore
        ///
        /// <summary>
        /// 移動操作を実行します。
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private bool MoveCore(IInformation src, IInformation dest)
            => Action(nameof(Move), () =>
        {
            var dir = dest.DirectoryName;
            if (!_core.Get(dir).Exists) _core.CreateDirectory(dir);
            _core.Move(src.FullName, dest.FullName);
        }, src.FullName, dest.FullName);

        /* ----------------------------------------------------------------- */
        ///
        /// Action
        ///
        /// <summary>
        /// 操作を実行します。
        /// </summary>
        /// 
        /// <remarks>
        /// 操作に失敗した場合、イベントハンドラで Cancel が設定されるまで
        /// 実行し続けます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private bool Action(string name, Action action, params string[] paths)
        {
            while (true)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception err)
                {
                    var args = new FailedEventArgs(name, paths, err);
                    OnFailed(args);
                    if (args.Cancel) return false;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Func
        ///
        /// <summary>
        /// 操作を実行します。
        /// </summary>
        /// 
        /// <remarks>
        /// 操作に失敗した場合、イベントハンドラで Cancel が設定されるまで
        /// 実行し続けます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        private T Func<T>(string name, Func<T> func, params string[] paths)
        {
            while (true)
            {
                try { return func(); }
                catch (Exception err)
                {
                    var args = new FailedEventArgs(name, paths, err);
                    OnFailed(args);
                    if (args.Cancel) return default(T);
                }
            }
        }

        #region Fields
        private IOperatorCore _core;
        #endregion

        #endregion
    }
}