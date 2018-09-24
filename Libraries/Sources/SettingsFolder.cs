﻿/* ------------------------------------------------------------------------- */
//
// Copyright (c) 2010 CubeSoft, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
/* ------------------------------------------------------------------------- */
using Cube.DataContract;
using Cube.FileSystem.Mixin;
using Cube.Log;
using Cube.Tasks;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace Cube.FileSystem
{
    /* --------------------------------------------------------------------- */
    ///
    /// SettingsFolder(T)
    ///
    /// <summary>
    /// ユーザ設定を保持するためのクラスです。
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class SettingsFolder<T> : IDisposable, INotifyPropertyChanged
        where T : INotifyPropertyChanged, new()
    {
        #region Constructors

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly) :
            this(assembly, Format.Registry) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        /// <param name="format">設定情報の保存方法</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly, Format format) :
            this(assembly, format, new IO()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        /// <param name="format">設定情報の保存方法</param>
        /// <param name="io">I/O オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly, Format format, IO io)
        {
            _dispose = new OnceAction<bool>(Dispose);
            Initialize(format, assembly.GetReader(), io);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        /// <param name="location">設定情報の保存場所</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly, string location) :
            this(assembly, Format.Registry, location) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        /// <param name="format">設定情報の保存形式</param>
        /// <param name="location">設定情報の保存場所</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly, Format format, string location) :
            this(assembly, format, location, new IO()) { }

        /* ----------------------------------------------------------------- */
        ///
        /// SettingsFolder(T)
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /// <param name="assembly">アセンブリ情報</param>
        /// <param name="format">設定情報の保存形式</param>
        /// <param name="location">設定情報の保存場所</param>
        /// <param name="io">I/O オブジェクト</param>
        ///
        /* ----------------------------------------------------------------- */
        public SettingsFolder(Assembly assembly, Format format, string location, IO io)
        {
            _dispose = new OnceAction<bool>(Dispose);
            Initialize(format, location, assembly.GetReader(), io);
        }

        #endregion

        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Value
        ///
        /// <summary>
        /// 設定内容を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public T Value { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Format
        ///
        /// <summary>
        /// 設定情報の保存形式を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Format Format { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Location
        ///
        /// <summary>
        /// 設定情報の保存場所を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Location { get; set; }

        /* ----------------------------------------------------------------- */
        ///
        /// IO
        ///
        /// <summary>
        /// I/O オブジェクトを取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public IO IO { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// バージョン情報を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public SoftwareVersion Version { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Company
        ///
        /// <summary>
        /// アプリケーションの発行者を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Company { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Product
        ///
        /// <summary>
        /// アプリケーション名を取得または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Product { get; private set; }

        /* ----------------------------------------------------------------- */
        ///
        /// Startup
        ///
        /// <summary>
        /// スタートアップ設定を取得します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Startup Startup { get; } = new Startup();

        /* ----------------------------------------------------------------- */
        ///
        /// AutoSave
        ///
        /// <summary>
        /// ユーザ毎の設定を自動的に保存するかどうかを示す値を取得または
        /// 設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool AutoSave { get; set; } = false;

        /* ----------------------------------------------------------------- */
        ///
        /// AutoSaveDelay
        ///
        /// <summary>
        /// 自動的保存の実行遅延時間を取得または設定します。
        /// </summary>
        ///
        /// <remarks>
        /// AutoSave モードの場合、短時間に大量の保存処理が実行される
        /// 可能性があります。SettingsFolder では、直前のプロパティの
        /// 変更から一定時間保存を保留する事で、これらの問題を回避します。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public TimeSpan AutoSaveDelay { get; set; } = TimeSpan.FromSeconds(1);

        #endregion

        #region Events

        #region PropertyChanged

        /* ----------------------------------------------------------------- */
        ///
        /// PropertyChanged
        ///
        /// <summary>
        /// プロパティの内容が変更された時に発生するイベントです。
        /// </summary>
        ///
        /// <remarks>
        /// この PropertyChanged イベントは Value.PropertyChanged
        /// イベントを補足して中継するために使用されます。
        /// </remarks>
        ///
        /* ----------------------------------------------------------------- */
        public event PropertyChangedEventHandler PropertyChanged;

        /* ----------------------------------------------------------------- */
        ///
        /// OnPropertyChanged
        ///
        /// <summary>
        /// PropertyChangd イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(this, e);

        #endregion

        #region Loaded

        /* ----------------------------------------------------------------- */
        ///
        /// Loaded
        ///
        /// <summary>
        /// 読み込み時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event ValueChangedEventHandler<T> Loaded;

        /* ----------------------------------------------------------------- */
        ///
        /// OnLoaded
        ///
        /// <summary>
        /// Loaded イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnLoaded(ValueChangedEventArgs<T> e)
        {
            if (e.OldValue != null) e.OldValue.PropertyChanged -= WhenChanged;
            if (e.NewValue != null) e.NewValue.PropertyChanged += WhenChanged;
            Value = e.NewValue;
            Loaded?.Invoke(this, e);
        }

        #endregion

        #region Saved

        /* ----------------------------------------------------------------- */
        ///
        /// Saved
        ///
        /// <summary>
        /// 自動保存機能が実行された時に発生するイベントです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public event KeyValueEventHandler<Format, string> Saved;

        /* ----------------------------------------------------------------- */
        ///
        /// OnSaved
        ///
        /// <summary>
        /// Saved イベントを発生させます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void OnSaved(KeyValueEventArgs<Format, string> e)
        {
            if (e.Key == Format.Registry) e.Key.Serialize(e.Value, Value);
            else IO.Save(e.Value, ss => e.Key.Serialize(ss, Value));

            Startup.Save();
            Saved?.Invoke(this, e);
        }

        #endregion

        #endregion

        #region Methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// ユーザ設定を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Load() => LoadOrDefault(default(T));

        /* ----------------------------------------------------------------- */
        ///
        /// LoadOrDefault
        ///
        /// <summary>
        /// ユーザ設定を読み込みます。
        /// </summary>
        ///
        /// <param name="alternate">失敗時に適用する値</param>
        ///
        /* ----------------------------------------------------------------- */
        public void LoadOrDefault(T alternate) =>
            OnLoaded(ValueChangedEventArgs.Create(Value, LoadCore(alternate)));

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// ユーザ設定をレジストリへ保存します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Save() =>
            OnSaved(KeyValueEventArgs.Create(Format, Location));

        #region IDisposable

        /* ----------------------------------------------------------------- */
        ///
        /// ~SettingsFolder
        ///
        /// <summary>
        /// オブジェクトを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        ~SettingsFolder() { _dispose.Invoke(false); }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public void Dispose()
        {
            _dispose.Invoke(true);
            GC.SuppressFinalize(this);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Dispose
        ///
        /// <summary>
        /// リソースを破棄します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _autosaver.Dispose();
            if (AutoSave) Save();
        }

        #endregion

        #endregion

        #region Implementations

        /* ----------------------------------------------------------------- */
        ///
        /// Initialize
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Initialize(Format fmt, AssemblyReader asm, IO io)
        {
            var root = fmt != Format.Registry ?
                       Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) :
                       string.Empty;
            var path = io.Combine(root, $@"{asm.Company}\{asm.Product}");

            Initialize(fmt, path, asm, io);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Initialize
        ///
        /// <summary>
        /// オブジェクトを初期化します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Initialize(Format fmt, string path, AssemblyReader asm, IO io)
        {
            Format   = fmt;
            Location = path;
            IO       = io;
            Version  = new SoftwareVersion(asm.Assembly);
            Company  = asm.Company;
            Product  = asm.Product;

            Value = new T();
            Value.PropertyChanged += WhenChanged;

            _autosaver.AutoReset = false;
            _autosaver.Elapsed += (s, e) => TaskEx.Run(() => Save()).Forget();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadCore
        ///
        /// <summary>
        /// 設定情報を読み込みます。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private T LoadCore(T error)
        {
            try
            {
                return Format == Format.Registry ?
                       Format.Deserialize<T>(Location) :
                       IO.Load(Location, ss => Format.Deserialize<T>(ss), error);
            }
            catch (Exception err)
            {
                this.LogWarn(err.ToString(), err);
                return error;
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WhenChanged
        ///
        /// <summary>
        /// Value.PropertyChanged イベントが発生した時に実行される
        /// ハンドラです。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void WhenChanged(object sener, PropertyChangedEventArgs e)
        {
            try
            {
                _autosaver.Stop();

                if (!AutoSave) return;
                if (AutoSaveDelay > TimeSpan.Zero)
                {
                    _autosaver.Interval = AutoSaveDelay.TotalMilliseconds;
                    _autosaver.Start();
                }
                else Save();
            }
            finally { OnPropertyChanged(e); }
        }

        #endregion

        #region Fields
        private readonly OnceAction<bool> _dispose;
        private readonly Timer _autosaver = new Timer();
        #endregion
    }
}
