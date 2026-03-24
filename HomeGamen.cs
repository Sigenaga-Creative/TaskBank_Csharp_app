using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Runtime.Remoting.Messaging;
using WMPLib;
using AxWMPLib;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;


namespace SYUUKAN
{
    //ロード処理
    public partial class Home : Form
    {

        //グローバルエリア
        private NomaruForm _nomaruForm = null;
        private Option _optionForm = null;
        private BonusForm _bonasuForm = null;
        private HardForm _hardForm = null;
        private wishlistForm _wishlistForm = null;
        private string Button = null;
        public int GenMoney;   //使っていい金額
        private bool TasseiDaily;    //デイリーが達成したか記録する変数                                     
        public Random random = new Random();   //ランダムの定義
        public string DailyTask;       //デイリー内容保存
        public bool DailyLock;         //デイリー内容のロック状態保持
        public bool SeviceLock;

        public class ServiceTask
        {
            public string TaskName { get; set; }
            public int Reward { get; set; }
            public bool Lock { get; set; }
        }     //まとめて管理
        //SeviceTaskをtaskとして使えるようにする
        public ServiceTask task = new ServiceTask();



        //音楽リストの保存
        string[] MusicList = {
            "1_garden.mp3", "2_おなじみの風景.mp3", "3_くるくるぽん.mp3",
            "4_なんということはない日常.mp3", "5_ゆるふわゲーム.mp3",
            "6_休み時間.mp3", "7_他愛のない話.mp3", "8_平穏な日々.mp3"
        };

        private int TrackIndex = 0;
        private bool isShuffle,isPlay;  // ON/OFFフラグ
        private List<int> ShuffleOrder = new List<int>();  // シャッフル順序を保持
        private int lastIndex = -1;
        

        public Home()
        {

            InitializeComponent();

            
            //画面の設定(普通の処理)
            this.Text = "";
            this.WindowState = FormWindowState.Maximized;      //最大画面変更
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.TopMost = false;       //ここは他アプリより優先するか調べている
            

#if DEBUG
            // テスト用：日付の調整(AddDaysの中で調整化：例：-1なら1日スキップ)
            //CommonDB.UpdateLastDay(DateTime.Today.AddDays(-1));         //デイリー
            //CommonDB.SaveServiceLastDay(DateTime.Today.AddDays(-1));    //ミッション
            // ※上2行のコメントを解除するとデイリーとミッションのみ設定を変えられます。
            // (ボーナスは対象外です)
#endif


            //データベースから持ってくる処理
            TasseiDaily = CommonDB.GetDaily();      //デイリーが達成してたか持ってくる
            GenMoney_C();       //残金の更新

            (string task, bool locked) DailyInfo = CommonDB.GetDailyTask();
            DailyTask = DailyInfo.task;
            DailyLock = DailyInfo.locked;

            (isShuffle, isPlay) = CommonDB.GetMusicSettings(); //音楽の設定を取って来る


            //音楽系の設定
            Kyoku.uiMode = "mini";
            Kyoku.Location = new Point(-1000, -1000);
            Kyoku.Size = new Size(1, 1);
            ButtonOto.uiMode = "mini";
            ButtonOto.Location = new Point(-1005, -1005);
            ButtonOto.Size = new Size(1, 1);

            ShuffleMusic(); // ←最初に一度シャッフル順を作る

            if (isShuffle)
            {
                TrackIndex = 0; // シャッフル順の最初から(動作をわかりやすくするために分けている)
                MusicMode.Image = Image.FromFile("曲シャッフル.png");

            }
            else
            {
                TrackIndex = 0; // 順番通りリストの最初から
                MusicMode.Image = Image.FromFile("曲リピート.png");

            }

            if (isPlay)
            {
                //再生したい
                Playback.Image = Image.FromFile("曲停止.png");
                PlayMusic(GetMusicIndex());
            }
            else
            {
                //再生したくない
                Playback.Image = Image.FromFile("曲再生.png");
            }


            //画面を飾る処理
            Toziru.Image = Image.FromFile("閉じるボタン.png");
            Deiri.Image = Image.FromFile("デイリー.png");
            Deiri_H.Image = Image.FromFile("デイリー背景.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");
            Chara.Image = Image.FromFile("ベース.png");
            TukaKin_H.Image = Image.FromFile("使える金額_背景色.png");
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Bonasu.Image = Image.FromFile("ボーナス.png");
            Hard.Image = Image.FromFile("ハード.png");
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            Hyaku.Image = Image.FromFile("百.png");
            Sen.Image = Image.FromFile("千.png");
            Man.Image = Image.FromFile("万.png");
            Kesu.Image = Image.FromFile("削除ボタン.png");
            Tassei_M.Image = Image.FromFile("横達成ボタン.png");
            Serihu.Image = Image.FromFile("通常セリフ.png");
            Kakutei.Image = Image.FromFile("横確定ボタン.png");
            KyokuNext.Image = Image.FromFile("曲飛ばし.png");
            KyokuReturn.Image = Image.FromFile("曲戻し.png");
            MogiHaikei_M.Image = Image.FromFile("ミッションの文字背景.png");
            Tassei_DM.Image = Image.FromFile("達成度_D&M.png");
            Syouhi.Image = Image.FromFile("消費.png");
            Hint_M.Image = Image.FromFile("ハテナボタン.png");





            //日付と時間のラベル更新
            DateTime now = DateTime.Now;
            Day.Text = now.ToString("HH:mm\nyyyy/MM/dd");

            DateTime today = DateTime.Today;
            DateTime lastDay = CommonDB.GetLastDay();




            //左上端に今月を表示する
            int month = DateTime.Now.Month;
            TukiLabel.Text = $"{month}月";

            //使う金額の初期設定
            CommonFunction.MoziEvent(Tukau_tx, "0");   //テキストボックスのイベント追加
            CommonFunction.SetMozi(Tukau_tx, "0");

            //セリフを設定する処理
            ChangeSerihuSet();





        }

        //描画後の処理
        private void Form1_Load(object sender, EventArgs e)
        {
            //デイリーとペナルティチェック
            DateTime today = DateTime.Today;    //その日を取得
            DateTime lastDay = CommonDB.GetLastDay();   //前に保存した状態を取得           
            //日付が変わったら実行
            if (today.Date != lastDay.Date)
            {
                //月が変わっているかチェック
                CommonDB.CheckMonth();

                int DayMiss = (today.Date - lastDay.Date).Days;     //何日ログインしていないか差を比べる
                int totalMiss = 0;
                string Kaimono = SerihuKaimono();
                int reward = CommonDB.ALLRewardGet.GetRewardValue("DailyReward");

                //それぞれの日数に応じて変化
                switch (DayMiss)
                {

                    case 1:
                        //一日目の場合(普通の処理)
                        Daily_Reverse(TasseiDaily);     //達成しているかチェック。
                        break;
                    case 2:
                        //二日空いてる場合
                        totalMiss = DayMiss * reward;      //デイリー金額×忘れた日数だけペナルティ
                        GenMoney -= totalMiss;
                        // キャラクター処理
                        Chara.Image = Image.FromFile("喜び.png");
                        SerihuLabel.Text = $"あれ、昨日ログインし忘れました？\n実は{totalMiss}円を{Kaimono}に使っちゃいました...！すみません～！";
                        CommonDB.HerasuMoney(totalMiss); // DBにも反映
                        break;
                    case 3:
                        //三日空いてる場合

                        totalMiss = DayMiss * reward;
                        GenMoney -= (totalMiss + 200);

                        CommonDB.HerasuMoney(totalMiss + 200); // DBにも反映

                        // キャラクター処理
                        Chara.Image = Image.FromFile("おこ.png");
                        SerihuLabel.Text = $"三日間なんでこなかったんですか...？\nそんな人にはお小遣い{totalMiss + 200}円抜きです！";

                        break;
                    default:
                        //四日以上空いている場合
                        totalMiss = DayMiss * reward;
                        GenMoney -= (totalMiss + 400);

                        CommonDB.HerasuMoney(totalMiss + 400); // DBにも反映

                        // キャラクター処理
                        Chara.Image = Image.FromFile("おこ.png");
                        SerihuLabel.Text = $"あ、{DayMiss}日ぶりですね。すごく暇だったので、{totalMiss + 400}円を{Kaimono}に使っちゃいました。\nまあいいですよね？";
                        break;
                }
                GenMoney_C();       //プラスマイナスチェック
                CommonDB.UpdateLastDay(today);  //今日の日付を更新
                Tassei_D.Image = Image.FromFile("達成ボタン.png");
                TasseiDaily = false;
                CommonDB.TasseiDailyDB(TasseiDaily);
            }
            //デイリーの達成状態に合わせて画像を変える
            if (CommonDB.GetDaily() == true)
            {
                Tassei_D.Image = Image.FromFile("達成ボタン_カーソル後.png");
                D.Image = Image.FromFile("完.png");
            }
            else
            {
                Tassei_D.Image = Image.FromFile("達成ボタン.png");
                D.Image = Image.FromFile("未.png");
            }

            DailyLabel.Text = DailyTask;    //デイリーの内容を表示

            //↓↓ついでにここでミッションの設定
            DateTime? lastBonusDay = CommonDB.GetServiceLastDay();      //最後に達成したのがいつか取得

            // すでに今日実行済みならスキップ(ノーマルとボーナスからランダムに取得し報酬金額とロック状態を渡す)
            if (!lastBonusDay.HasValue || lastBonusDay.Value.Date != today)
            {
                // 実行日を保存
                CommonDB.SaveServiceLastDay(today);

                // ノーマル＋ボーナスのタスクを取得
                var Tasks = CommonDB.GetTasks();
                var RandomTask = Tasks[random.Next(Tasks.Count)]; //ノーマルボーナスのタスクを数えてランダムへ

                int ServiceMoney = 0;
                // ミッションの報酬金額の追加
                if (RandomTask.Reward <= 300)
                {
                    //元の報酬金額が300円以下なら
                    ServiceMoney = RandomTask.Reward + 200; // 200円上乗せ
                }
                else if (RandomTask.Reward <= 600)
                {
                    //400円以上600円以下なら
                    ServiceMoney = RandomTask.Reward + 400; // 400円上乗せ
                }
                else
                {
                    //700円以上なら
                    ServiceMoney = RandomTask.Reward + 600; // 600円上乗せ
                }
                CommonDB.UpdateServiceInfo(RandomTask.TaskName, false, ServiceMoney);
            }
            var task = CommonDB.GetServiceInfo();
            SeviceLock = task.Lock;
            if(SeviceLock == true)
            {
                Tassei_M.Image = Image.FromFile("横達成ボタン_カーソル後.png");
                M.Image = Image.FromFile("完.png");

            }
            else
            {
                Tassei_M.Image = Image.FromFile("横達成ボタン.png");
                M.Image = Image.FromFile("未.png");

            }






            label3.Text = task.TaskName;





            //時間を定義
            timer1.Interval = 5000; // 5秒ごとに実行
            timer1.Tick += timer1_Tick;
            timer1.Enabled = true;

            timer2.Interval = 3000000; // 5分ごとに実行
            timer2.Tick += timer2_Tick;
            timer2.Start();

            timer3.Interval = 750; // 0.75秒ごとに実行
            timer3.Enabled = true;

        }

        //######################################【関数エリア】###########################################

        //設定フォーム以外削除
        private void OptionForm_K()
        {
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Bonasu.Image = Image.FromFile("ボーナス.png");
            Hard.Image = Image.FromFile("ハード.png");
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");

            //追加処理↓
            //IsDisposed　←画面が表示されているかチェック
            if (_nomaruForm != null && !_nomaruForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _nomaruForm.Close();

                //グローバルのフォームをnullにリセット
                _nomaruForm = null;
            }
            //ボーナス画面があれば閉じる
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _bonasuForm.Close();

                //グローバルのフォームをnullにリセット
                _bonasuForm = null;
            }
            //ハード画面があれば閉じる
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _hardForm.Close();

                //グローバルのフォームをnullにリセット
                _hardForm = null;
            }
            //ウォッシュリスト
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _wishlistForm.Close();

                //グローバルのフォームをnullにリセット
                _wishlistForm = null;
            }


        }

        //ノーマルフォーム以外削除
        private void NomaruForm_K()
        {
            Bonasu.Image = Image.FromFile("ボーナス.png");
            Hard.Image = Image.FromFile("ハード.png");
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");

            //設定画面があれば閉じる
            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                //画像切り替え
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                // 既に開いている場合：閉じる処理を実行
                _optionForm.Close();

                //グローバルのフォームをnullにリセット
                _optionForm = null;
            }
            //ボーナス画面があれば閉じる
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _bonasuForm.Close();

                //グローバルのフォームをnullにリセット
                _bonasuForm = null;
            }
            //ハード画面があれば閉じる
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _hardForm.Close();

                //グローバルのフォームをnullにリセット
                _hardForm = null;
            }
            //ウォッシュリスト
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _wishlistForm.Close();

                //グローバルのフォームをnullにリセット
                _wishlistForm = null;
            }

        }

        //ボーナスフォーム以外削除
        private void BonasuForm_K()
        {
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Hard.Image = Image.FromFile("ハード.png");
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");

            //設定画面があれば閉じる
            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                _optionForm.Close();

                _optionForm = null;
            }
            //ノーマル画面があれば閉じる
            if (_nomaruForm != null && !_nomaruForm.IsDisposed)
            {
                _nomaruForm.Close();

                _nomaruForm = null;
            }
            //ハード画面があれば閉じる
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                _hardForm.Close();

                _hardForm = null;
            }
            //ウォッシュリスト
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                _wishlistForm.Close();

                _wishlistForm = null;
            }

        }

        //ハードフォーム以外削除
        private void HardForm_K()
        {
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Bonasu.Image = Image.FromFile("ボーナス.png");
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");

            //ノーマル画面があれば閉じる
            if (_nomaruForm != null && !_nomaruForm.IsDisposed)
            {
                // 既に開いている場合：閉じる処理を実行
                _nomaruForm.Close();

                //グローバルのフォームをnullにリセット
                _nomaruForm = null;
            }
            //設定画面があれば閉じる
            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                _optionForm.Close();

                _optionForm = null;
            }
            //ボーナス画面があれば閉じる
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                _bonasuForm.Close();

                _bonasuForm = null;
            }
            //ウォッシュリスト
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                _wishlistForm.Close();

                _wishlistForm = null;
            }

        }

        //wishlistフォーム以外削除
        private void wishlistForm_K()
        {
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Bonasu.Image = Image.FromFile("ボーナス.png");
            Hard.Image = Image.FromFile("ハード.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");

            //設定画面があれば閉じる
            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                _optionForm.Close();

                _optionForm = null;
            }
            //ボーナス画面があれば閉じる
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                _bonasuForm.Close();

                _bonasuForm = null;
            }
            //ハード画面があれば閉じる
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                _hardForm.Close();

                _hardForm = null;
            }
            //ノーマル画面があれば閉じる
            if (_nomaruForm != null && !_nomaruForm.IsDisposed)
            {
                _nomaruForm.Close();

                _nomaruForm = null;
            }

        }

        //全てのフォームを削除
        private void ALLForm_K()
        {
            wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            Bonasu.Image = Image.FromFile("ボーナス.png");
            Hard.Image = Image.FromFile("ハード.png");
            Opthion.Image = Image.FromFile("詳細ボタン.png");

            //ウォッシュリスト
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                _wishlistForm.Close();

                _wishlistForm = null;
            }
            //設定画面があれば閉じる
            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                _optionForm.Close();

                _optionForm = null;
            }
            //ボーナス画面があれば閉じる
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                _bonasuForm.Close();

                _bonasuForm = null;
            }
            //ハード画面があれば閉じる
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                _hardForm.Close();

                _hardForm = null;
            }
            //ノーマル画面があれば閉じる
            if (_nomaruForm != null && !_nomaruForm.IsDisposed)
            {
                _nomaruForm.Close();

                _nomaruForm = null;
            }
        }

        //使える金がプラスかマイナスか調べて色を変える
        public void TukaeruMoney_iro(int Tukaeru)
        {
            if (Tukaeru < 0)
            {
                //マイナスになれば「赤」表示
                Tukaeru_Kane.ForeColor = Color.FromArgb(255, 192, 192);
            }
            else
            {
                //プラスであれば「青」表示
                Tukaeru_Kane.ForeColor = Color.FromArgb(192, 192, 255);
            }
        }

        //現在の残金を取得し、色やラベルを更新する
        public void GenMoney_C()
        {
            GenMoney = CommonDB.GenMoney();     //現在の使える金額をデータベースから取得
            Tukaeru_Kane.Text = $"{GenMoney.ToString("N0")}";    //使える金額のラベルを更新
            TukaeruMoney_iro(GenMoney);     //+-のチェックと色変更
        }

        //達成したかチェック
        private bool Daily_Reverse(bool TasseiDaily)
        {
            if (TasseiDaily == true)
            {
                // キャラクター処理
                Chara.Image = Image.FromFile("ベース.png");
                SerihuLabel.Text = "昨日はデイリー達成しましたね！\n今日もその調子でいきましょう～！";


            }
            else
            {
                //主に時間経過で達成しなかった場合を想定
                int reward = CommonDB.ALLRewardGet.GetRewardValue("DailyReward");
                GenMoney -= reward;
                CommonDB.HerasuMoney(reward);

                // キャラクター処理
                Chara.Image = Image.FromFile("通常.png");
                SerihuLabel.Text = $"あ～、残念ですけど昨日デイリー未達成なので{reward}円引かれました。次できれば取り返せますよ！";


                // 状態リセット
                TasseiDaily = false;
                CommonDB.TasseiDailyDB(TasseiDaily);    //データベースに保存
                Tassei_D.Image = Image.FromFile("達成ボタン.png");
            }

            // 使える金額を更新
            GenMoney_C();
            return TasseiDaily;


        }


        //時間ごとに変わるセリフと共通のセリフを7:3の比率で設定
        private void ChangeSerihuSet()
        {
            (string, string)[] commonSet = new (string, string)[]
            {
                ("がんばりすぎると、身体に毒ですよ！適度に休むことで集中力も維持しやすくなります。", "通常.png"),
                ("そちらは晴れていますか？\n私、晴れていると気分があがるんです！", "喜び.png"),
                ("今あるタスクは逃げません！\n自分のペースで達成していきましょう！", "ベース.png"),
                ("今日のミッションはこちら！\n時間を見つけてやってみましょう！", "ベース.png")
            };

            //時間の取得
            int hour = DateTime.Now.Hour;

            (string text, string imagePath)[] timeSet;

            if (hour >= 6 && hour < 11)
            {
                // 🌅 朝
                timeSet = new (string, string)[]
                {
                    ("おはようございます！今日も一日がんばりましょう！", "ベース.png"),
                    ("朝日見ました？きっと綺麗ですよ！", "喜び.png"),
                    ("忘れ物してませんか？一度確認しておきましょう。", "通常.png")
                };
            }
            else if (hour >= 11 && hour < 16)
            {
                // 🌞 昼
                timeSet = new (string, string)[]
                {
                    ("お昼ごはんはもう食べました？午後も頑張りましょう～！", "喜び.png"),
                    ("眠たくなったら、お昼寝すると作業がはかどるらしいです！", "通常.png"),
                    ("今休憩時間ですか？\nいいですね！\nしっかり休みましょう！", "ベース.png")
                };
            }
            else if (hour >= 16 && hour < 21)
            {
                // 🌇 夕方
                timeSet = new (string, string)[]
                {
                    ("もうすぐ一日が終わりますね。そろそろ息抜きの時間です！", "ベース.png"),
                    ("お疲れさまです！今日の成果を振り返っておきましょう。", "通常.png"),
                    ("一日のご褒美になにか買っておきます？買うなら私の分もお願いします！", "喜び.png")
                };
            }
            else
            {
                // 🌙 夜
                timeSet = new (string, string)[]
                {
                    ("そろそろおやすみタイムですね。早く寝ると良いことたくさんですよ。", "通常.png"),
                    ("おやすみ前に、タスクの確認でもしておきます？", "ベース.png"),
                    ("あんまり寝るのが遅いとお体に障りますよ！\nもう寝ましょう！！", "おこ.png")
                };
            }
            // ランダムで「時間帯 or 共通」を選ぶ
            double chance = random.NextDouble(); // 0.0～1.0のランダム値

            //chanceの値が0.7以下だったらtimeSet、それ以外ならcommonSet
            (string, string)[] RandomSerihuSet = (chance < 0.7) ? timeSet : commonSet;

            // その中からさらに1つ選んで表示
            int index = random.Next(RandomSerihuSet.Length);
            SerihuLabel.Text = RandomSerihuSet[index].Item1;
            Chara.Image = Image.FromFile(RandomSerihuSet[index].Item2);

        }

        //お金を増やすときにランダムでセリフを変える処理　　必要なもの→　home.HuyasuSerihuSet(name, reward);
        public void HuyasuSerihuSet(string name,int reward)
        {
            (string, string)[] commonSet = new (string, string)[]
            {
                ($"おぉ～！「{name}」を達成したんですね！\nお小遣いが{reward}円増えてますよ！", "喜び.png"),
                ($"お疲れさまです～！\n「{name}」を達成しました！\nお小遣いが{reward}円増えています！", "ベース.png"),
                ($"あれ？「{name}」を達成してますね。\nお小遣いも{reward}円増やしておきますね！", "通常.png"),

            };

            int index = random.Next(commonSet.Length);
            SetSerihu(commonSet[index].Item1, commonSet[index].Item2);


        }
        //ウォッシュリスト用セリフ(使用モード時)
        public void TukauSerihuSet(string name, int reward)
        {
            (string, string)[] commonSet = new (string, string)[]
            {
                ($"「{name}」をやったんですね！\nお小遣いにも{reward}円分反映されてます。", "通常.png"),
                ($"やりたいこと「{name}」を達成しました！\n{reward}円減ってますけど、理想に近づいているはずです！", "ベース.png"),
            };

            int index = random.Next(commonSet.Length);
            SetSerihu(commonSet[index].Item1, commonSet[index].Item2);


        }

        //お金を減らすときにランダムでセリフを変える処理
        public void HerasuSerihuSet(int Tukau)
        {
            (string, string)[] commonSet = new (string, string)[]
            {
                ($"お小遣いを{Tukau}円使用しました！\n誰かにプレゼントですか？", "通常.png"),
                ($"あぁー！私のへそくり{Tukau}円\n使いましたよね？\nえ、私のじゃない？", "おこ.png"),
                ($"おぉ～！お小遣いを{Tukau}円使ったんですね！\n自分を甘やかしちゃいましょう～！", "喜び.png"),


            };

            int index = random.Next(commonSet.Length);
            SetSerihu(commonSet[index].Item1, commonSet[index].Item2);


        }

        public string SerihuKaimono()
        {
            string[] Kaimono =
 {
                ("スイーツ"),
                ("お菓子"),
                ("果物"),
                ("生活用品"),
                ("趣味"),
                ("ゲームの課金"),
                ("募金活動している団体"),
                ("飲み物"),
            };
            int index = random.Next(Kaimono.Length);
 
            return Kaimono[index];

        }


        //セリフとイラストを変える処理
        public void SetSerihu(string message,string image)
        {
            SerihuLabel.Text = message;
            Chara.Image = Image.FromFile(image);

        }

        //音楽を流す設定↓↓↓↓
        private void PlayMusic(int KyokuIndex)
        {
            //音楽を流す処理
            Kyoku.URL = MusicList[KyokuIndex];
            if (isPlay == true)
                Kyoku.Ctlcontrols.play();

            //ラベルを更新する処理
            UpdateLabel();
        }

        //次の音楽
        private void NextMusic()
        {
            if (isShuffle)
            {
                TrackIndex++;
                if (TrackIndex >= ShuffleOrder.Count)
                {
                    // すべて再生済み → 新しいシャッフルリストを作成
                    ShuffleMusic();
                    TrackIndex = 0;
                }
            }
            else
            {
                TrackIndex = (TrackIndex + 1) % MusicList.Length;
            }

            PlayMusic(GetMusicIndex());
        }

        //音楽を戻す処理
        private void PrevMusic()
        {
            if (isShuffle)
            {
                TrackIndex--;
                if (TrackIndex < 0)
                    TrackIndex = ShuffleOrder.Count - 1;
            }
            else
            {
                //順次再生の場合
                TrackIndex = (TrackIndex - 1 + MusicList.Length) % MusicList.Length;
            }

            PlayMusic(GetMusicIndex());
        }

        //曲のランダム
        private void ShuffleMusic()
        {

            ShuffleOrder.Clear();
            ShuffleOrder.AddRange(Enumerable.Range(0, MusicList.Length));

            // Fisher-Yates シャッフル
            for (int i = ShuffleOrder.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (ShuffleOrder[i], ShuffleOrder[j]) = (ShuffleOrder[j], ShuffleOrder[i]);
            }

            // 直前の曲と被ったら入れ替え
            if (ShuffleOrder.Count > 1 && ShuffleOrder[0] == lastIndex)
            {
                (ShuffleOrder[0], ShuffleOrder[1]) = (ShuffleOrder[1], ShuffleOrder[0]);
            }
        }

        //現在再生している曲のインデックスを取得
        private int GetMusicIndex()
        {
            //isShuffle = trueであればシャッフルの中の引数(変更したやつ)　そうでないなら現在のTrackIndex
            return isShuffle ? ShuffleOrder[TrackIndex] : TrackIndex;
        }

        //音楽ラベルの更新
        private void UpdateLabel()
        {
            //MusicListの名前で取っている
            TrackLabel.Text = $"♪~{Path.GetFileNameWithoutExtension(MusicList[GetMusicIndex()])}"; 
        }

        //移動音を呼び出す関数
        public  void IdoOto()
        {
            ButtonOto.URL = "移動.mp3";
            ButtonOto.Ctlcontrols.play();
        }

        //決定音
        public void KetteiOto()
        {
            ButtonOto.URL = "決定.mp3";
            ButtonOto.Ctlcontrols.play();
        }


        //##############################################################################################


        //プログラム閉じる処理↓↓↓↓↓↓↓
        private void Togiru_Click(object sender, EventArgs e)
        {
            KetteiOto();
            //すべてのフォームを閉じる
            Application.Exit();
        }

        private void Togiru_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Toziru.Image = Image.FromFile("閉じるボタン_カーソル後.png");
        }

        private void Togiru_MouseLeave(object sender, EventArgs e)
        {
            Toziru.Image = Image.FromFile("閉じるボタン.png");
        }

        //############################################################

        //##################     設定    #############################
        private void Opthion_Click(object sender, EventArgs e)
        {
            KetteiOto();
            //これをしないと別に開いていたフォームがある場合、Buttonがリセットされていないことになる
            Button = null;

            if (_optionForm != null && !_optionForm.IsDisposed)
            {
                //オプション画面が表示されていたら
                OptionForm_K();

                //画像切り替え
                Opthion.Image = Image.FromFile("詳細ボタン.png");

                // 既に開いている場合：閉じる処理を実行
                _optionForm.Close();

                //グローバルのフォームをnullにリセット
                _optionForm = null;


            }
            else
            {
                //オプション画面が表示されていなかったら

                //画像切り替え
                Opthion.Image = Image.FromFile("詳細ボタン_カーソル後.png");

                _optionForm = new Option(this);

                //イベントを設置(デイリーの内容が更新されたら始動)
                _optionForm.DailyTaskChanged += (newDailyTask) =>
                {
                    // 画面上の表示を更新
                    DailyLabel.Text = newDailyTask;
                };

                //オプションフォームからイベントをうけたら実行
                //↓ラムダ式：形の省略。【「ここにあるものを実行する」 += (s,e2) => 】 という意味
                //s：どこがこのイベントを起こしたの？(この場合OptionFormのKakutei_Dなはず) e2:気にしなくてよし
                _optionForm.DailyMoneyChanged += (s, e2) =>
                {
                    //残金を更新し、色を変え、表示する(便利な関数作ってたの忘れてた)
                    GenMoney_C();
                };

                //Formのサイズ変更防止及び名前の非表示
                _optionForm.FormBorderStyle = FormBorderStyle.FixedSingle;    //スクロールなどで画面の変更防止
                _optionForm.FormBorderStyle = FormBorderStyle.None;   //コントロールボックスなくす(枠線の完全削除版)
                _optionForm.Height = this.ClientSize.Height;     //ホーム画面に高さを合わせる
                _optionForm.TopMost = true;  //常時最前面表示
                _optionForm.Text = "";       //プログラム名非表示

                _optionForm.StartPosition = FormStartPosition.Manual;   //下のコードで画面を設計するのに必要
                _optionForm.Location = new Point(0, 0); // 画面左上 (X=0, Y=0) に設定
                _optionForm.Show();

                OptionForm_K();
            }
        }

        //############################################################

        //デイリー確定処理↓↓↓↓↓↓↓
        private void Tassei_D_Click(object sender, EventArgs e)
        {
            //データベースから取得し、すでに達成しているなら押せない。
            if (CommonDB.GetDaily() == true)
            {
                return;
            }

            int reward = CommonDB.ALLRewardGet.GetRewardValue("DailyReward");
            GenMoney += reward;
            CommonDB.HuyasuMoney(reward);
            TasseiDaily = true;
            CommonDB.TasseiDailyDB(TasseiDaily);    //データベースに達成したことを保存

            // 使える金額を更新
            GenMoney_C();
            D.Image = Image.FromFile("完.png");


            // キャラクター処理
            Chara.Image = Image.FromFile("喜び.png");
            SerihuLabel.Text = $"お見事です！\nデイリー達成でお小遣いが{reward}円増えました！\n小さな積み重ねが大事ですよね！";

            KetteiOto();
        }

        private void Tassei_D_MouseEnter(object sender, EventArgs e)
        {

            if (TasseiDaily != true)
            {
                Tassei_D.Image = Image.FromFile("達成ボタン_カーソル後.png");
                IdoOto();
            }

        }

        private void Tassei_D_MouseLeave(object sender, EventArgs e)
        {
            if(TasseiDaily != true)
            {
                Tassei_D.Image = Image.FromFile("達成ボタン.png");
            }
        }

        //############################################################

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //              ↓↓↓↓     Nomaru    ↓↓↓↓
        private void Nomaru_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            if(Button != "Nomaru")
            {
                Nomaru.Image = Image.FromFile("ノーマルアイコン_カーソル後.png");
            }
        }

        private void Nomaru_MouseLeave(object sender, EventArgs e)
        {
            if(Button != "Nomaru")
            {
                Nomaru.Image = Image.FromFile("ノーマルアイコン.png");
            }

        }

        private void Nomaru_Click(object sender, EventArgs e)
        {
            KetteiOto();


            //追加処理↓
            //IsDisposed　←画面が表示されているかチェック
            if (_nomaruForm != null  && !_nomaruForm.IsDisposed)
            {
                Button = null;
                Nomaru.Image = Image.FromFile("ノーマルアイコン.png");

                NomaruForm_K();

                // 既に開いている場合：閉じる処理を実行
                _nomaruForm.Close();

                //グローバルのフォームをnullにリセット
                _nomaruForm = null;


            }
            else
            {
                Button = "Nomaru";
                Nomaru.Image = Image.FromFile("ノーマルアイコン_カーソル後.png");

                _nomaruForm = new NomaruForm(this);

                //Formのサイズ変更防止及び名前の非表示
                _nomaruForm.FormBorderStyle = FormBorderStyle.FixedSingle;    //スクロールなどで画面の変更防止
                _nomaruForm.FormBorderStyle = FormBorderStyle.None;   //コントロールボックスなくす(枠線の完全削除版)
                _nomaruForm.Height = this.ClientSize.Height;     //ホーム画面に高さを合わせる
                _nomaruForm.TopMost = true;  //常時最前面表示
                _nomaruForm.Text = "";       //プログラム名非表示

                _nomaruForm.StartPosition = FormStartPosition.Manual;
                _nomaruForm.Location = new Point(0, 0); // 画面左上 (X=0, Y=0) に設定
                _nomaruForm.Show();

                NomaruForm_K();

            }

        }

        //############################################################

        //              ↓↓↓↓     Bonasu    ↓↓↓↓
        private void Bonasu_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            if(Button != "Bonasu")
            {
                Bonasu.Image = Image.FromFile("ボーナス_カーソル後.png");
            }
        }

        private void Bonasu_MouseLeave(object sender, EventArgs e)
        {
            if (Button != "Bonasu")
            {
                Bonasu.Image = Image.FromFile("ボーナス.png");
            }
        }

        private void Bonasu_Click(object sender, EventArgs e)
        {
            KetteiOto();
            //追加処理
            if (_bonasuForm != null && !_bonasuForm.IsDisposed)
            {
                Button = null;
                Bonasu.Image = Image.FromFile("ボーナス.png");

                BonasuForm_K();

                _bonasuForm.Close();

                _bonasuForm = null;


            }
                else
            {
                Button = "Bonasu";
                Bonasu.Image = Image.FromFile("ボーナス_カーソル後.png");

                _bonasuForm = new BonusForm(this);

                //Formのサイズ変更防止及び名前の非表示
                _bonasuForm.FormBorderStyle = FormBorderStyle.FixedSingle;    //スクロールなどで画面の変更防止
                _bonasuForm.FormBorderStyle = FormBorderStyle.None;   //コントロールボックスなくす(枠線の完全削除版)
                _bonasuForm.Height = this.ClientSize.Height;     //ホーム画面に高さを合わせる
                _bonasuForm.TopMost = true;  //常時最前面表示
                _bonasuForm.Text = "";       //プログラム名非表示

                _bonasuForm.StartPosition = FormStartPosition.Manual;
                _bonasuForm.Location = new Point(0, 0); // 画面左上 (X=0, Y=0) に設定
                _bonasuForm.Show();

                BonasuForm_K();

            }
        }

        //############################################################

        //              ↓↓↓↓     Hard    ↓↓↓↓
        private void Hard_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            if (Button != "Hard")
            {
                Hard.Image = Image.FromFile("ハード_カーソル後.png");
            }
        }

        private void Hard_MouseLeave(object sender, EventArgs e)
        {
            if (Button != "Hard")
            {
                Hard.Image = Image.FromFile("ハード.png");
            }
        }

        private void Hard_Click(object sender, EventArgs e)
        {
            KetteiOto();

            //追加処理
            if (_hardForm != null && !_hardForm.IsDisposed)
            {
                Button = null;
                Hard.Image = Image.FromFile("ハード.png");

                HardForm_K();

                _hardForm.Close();

                _hardForm = null;

            }
            else
            {
                Button = "Hard";
                Hard.Image = Image.FromFile("ハード_カーソル後.png");

                _hardForm = new HardForm(this);

                //Formのサイズ変更防止及び名前の非表示
                _hardForm.FormBorderStyle = FormBorderStyle.FixedSingle;    //スクロールなどで画面の変更防止
                _hardForm.FormBorderStyle = FormBorderStyle.None;   //コントロールボックスなくす(枠線の完全削除版)
                _hardForm.Height = this.ClientSize.Height;     //ホーム画面に高さを合わせる
                _hardForm.TopMost = true;  //常時最前面表示
                _hardForm.Text = "";       //プログラム名非表示

                _hardForm.StartPosition = FormStartPosition.Manual;
                _hardForm.Location = new Point(0, 0); // 画面左上 (X=0, Y=0) に設定
                _hardForm.Show();

                HardForm_K();

            }
        }

        //############################################################

        //              ↓↓↓↓     Wishlist    ↓↓↓↓
        private void wishlist_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            if(Button != "Wishlist")
            wishlist.Image = Image.FromFile("ウォッシュリスト_カーソル後.png");
        }

        private void wishlist_MouseLeave(object sender, EventArgs e)
        {
            if (Button != "Wishlist")
            {
                wishlist.Image = Image.FromFile("ウォッシュリスト.png");
            }
        }

        private void wishlist_Click(object sender, EventArgs e)
        {
            KetteiOto();
           

            //追加処理
            if (_wishlistForm != null && !_wishlistForm.IsDisposed)
            {
                Button = null;
                wishlist.Image = Image.FromFile("ウォッシュリスト.png");
                wishlistForm_K();

                _wishlistForm.Close();

                _wishlistForm = null;

            }
            else
            {
                Button = "Wishlist";
                wishlist.Image = Image.FromFile("ウォッシュリスト_カーソル後.png");

                _wishlistForm = new wishlistForm(this);

                //Formのサイズ変更防止及び名前の非表示
                _wishlistForm.FormBorderStyle = FormBorderStyle.FixedSingle;    //スクロールなどで画面の変更防止
                _wishlistForm.FormBorderStyle = FormBorderStyle.None;   //コントロールボックスなくす(枠線の完全削除版)
                _wishlistForm.Height = this.ClientSize.Height;     //ホーム画面に高さを合わせる
                _wishlistForm.TopMost = true;  //常時最前面表示
                _wishlistForm.Text = "";       //プログラム名非表示

                _wishlistForm.StartPosition = FormStartPosition.Manual;
                _wishlistForm.Location = new Point(0, 0); // 画面左上 (X=0, Y=0) に設定
                _wishlistForm.Show();

                wishlistForm_K();

            }
        }

        //############################################################
        //############################################################

        //それぞれのボタンから使う金額を選ぶ処理↓↓↓↓↓↓↓
        private void Hyaku_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Hyaku.Image = Image.FromFile("百_カーソル後.png");

        }

        private void Hyaku_MouseLeave(object sender, EventArgs e)
        {
            Hyaku.Image = Image.FromFile("百.png");

        }

        private void Hyaku_Click(object sender, EventArgs e)
        {
            KetteiOto();

            int kazu;
            // 空欄なら 0 にする
            if (string.IsNullOrWhiteSpace(Tukau_tx.Text))
            {
                kazu = 0;
            }
            else
            {
                kazu = int.Parse(Tukau_tx.Text);
            }

            kazu = kazu + 100;
            Tukau_tx.Text = kazu.ToString();

        }

        private void Sen_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Sen.Image = Image.FromFile("千_カーソル後.png");
        }

        private void Sen_MouseLeave(object sender, EventArgs e)
        {
            Sen.Image = Image.FromFile("千.png");

        }

        private void Sen_Click(object sender, EventArgs e)
        {
            KetteiOto();
            int kazu;
            // 空欄なら 0 にする
            if (string.IsNullOrWhiteSpace(Tukau_tx.Text))
            {
                kazu = 0;
            }
            else
            {
                kazu = int.Parse(Tukau_tx.Text);
            }

            kazu = kazu + 1000;
            Tukau_tx.Text = kazu.ToString();

        }

        private void Man_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Man.Image = Image.FromFile("万_カーソル後.png");

        }

        private void Man_MouseLeave(object sender, EventArgs e)
        {
            Man.Image = Image.FromFile("万.png");

        }

        private void Man_Click(object sender, EventArgs e)
        {
            KetteiOto();
            int kazu;
            // 空欄なら 0 にする
            if (string.IsNullOrWhiteSpace(Tukau_tx.Text))
            {
                kazu = 0;
            }
            else
            {
                kazu = int.Parse(Tukau_tx.Text);
            }

            kazu = kazu + 10000;
            Tukau_tx.Text = kazu.ToString();
            
        }
        //############################################################

        //使う金額をリセットする処理↓↓↓↓↓↓↓
        private void Kesu_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Kesu.Image = Image.FromFile("削除ボタン_選択後.png");

        }

        private void Kesu_MouseLeave(object sender, EventArgs e)
        {
            Kesu.Image = Image.FromFile("削除ボタン.png");

        }

        private void Kesu_Click(object sender, EventArgs e)
        {
            KetteiOto();
            CommonFunction.SetMozi(Tukau_tx, "0");
        }
        //############################################################

        //ミッションのクリア確定処理↓↓↓↓↓↓↓
        private void Tassei_M_MouseEnter(object sender, EventArgs e)
        {
            if (SeviceLock != true)
            {
                Tassei_M.Image = Image.FromFile("横達成ボタン_カーソル後.png");
                IdoOto();
            }
        }

        private void Tassei_M_MouseLeave(object sender, EventArgs e)
        {
            if (SeviceLock != true)
            {
                Tassei_M.Image = Image.FromFile("横達成ボタン.png");
            }
        }

        private void Tassei_M_Click(object sender, EventArgs e)
        {
            var task = CommonDB.GetServiceInfo();


            // すでに今日実行済みならスキップ
            if (task.Lock == true)
            {
                return;
            }


            // お小遣いを増やす処理
            CommonDB.HuyasuMoney(task.Reward);


            // 使える金額を更新
            GenMoney_C();
            CommonDB.UpdateServiceInfo(task.TaskName, true, task.Reward);

            SeviceLock = true;

            M.Image = Image.FromFile("完.png");
            KetteiOto();
            // 表示
            // キャラクター処理
            Chara.Image = Image.FromFile("喜び.png");
            SerihuLabel.Text = $"『{task.TaskName}』を達成しました！\nお小遣いが{task.Reward}円増えています！\n確実に一歩進んでますよ！";

 
        }
        //############################################################

        //使う金額を確定させる処理↓↓↓↓↓↓↓
        private void Kakutei_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            Kakutei.Image = Image.FromFile("横確定ボタン_選択後.png");
        }

        private void Kakutei_MouseLeave(object sender, EventArgs e)
        {
            Kakutei.Image = Image.FromFile("横確定ボタン.png");

        }

        private void Kakutei_Click(object sender, EventArgs e)
        {
            KetteiOto();
            int Tukaeru = int.Parse(Tukaeru_Kane.Text.Replace(",", ""));
            int Tukau = int.Parse(Tukau_tx.Text);

            // 100の倍数チェック
            if (Tukau % 100 != 0)
            {
                MessageBox.Show("使う金額は100円単位で設定してください。",
                    "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // 一番近い100円単位に丸める
                Tukau = (Tukau / 100) * 100;
            }


            Tukaeru = Tukaeru - Tukau;

            //プラスかマイナスを判断して色を変える処理
            TukaeruMoney_iro(Tukaeru);


            Tukaeru_Kane.Text = Tukaeru.ToString("N0");

            //テキストボックスをリセット
            CommonFunction.SetMozi(Tukau_tx, "0");

            //データベースに更新
            CommonDB.KousinMoney(Tukaeru);

            //セリフチェンジ
            HerasuSerihuSet(Tukau);

        }

        //############################################################

        //秒数が短めな必要があるもの
        private void timer1_Tick(object sender, EventArgs e)
        {
            //ラベル更新
            DateTime now = DateTime.Now;
            Day.Text = now.ToString("HH:mm\nyyyy/MM/dd");

            DateTime today = DateTime.Today;
            DateTime lastDay = CommonDB.GetLastDay();


            //日付が一日以上経っていたら
            if (today.Date != lastDay.Date)
            {
                //月が変わっているかチェック
                CommonDB.CheckMonth();

                Daily_Reverse(TasseiDaily);     //達成しているか確認
                CommonDB.UpdateLastDay(today);  //今日の日付を更新

                (string task, bool locked) DailyInfo = CommonDB.GetDailyTask();
                DailyTask = DailyInfo.task;     //データベースからタスク(と達成度)持ってきて置き換え

                Tassei_D.Image = Image.FromFile("達成ボタン.png");
                TasseiDaily = false;
                CommonDB.TasseiDailyDB(TasseiDaily);
            }
        }

        //############################################################

        //セリフラベルを押すと次の会話が進む
        private void label4_Click(object sender, EventArgs e)
        {
            KetteiOto();
            ChangeSerihuSet();
        }

        //セリフが自動で変わる
        private void timer2_Tick(object sender, EventArgs e)
        {
            ChangeSerihuSet();
        }

        //############################################################

        private void Home_Click(object sender, EventArgs e)
        {
            //全てのフォームを閉じる
            ALLForm_K();

            //テキストボックスの選択を解除
            this.ActiveControl = null;
        }

        //使う金額の入力を数字以外受け付けなくする
        private void Tukau_tx_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        //ロード時にテキストボックスを選択するのを防ぐ(shownは読み込み終わってから実行される)
        private void Home_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
            Kyoku.PlayStateChange += Kyoku_PlayStateChange;     //曲自動化イベントの追加


        }

        //曲の自動切り替え
        private async void Kyoku_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {    
            switch ((WMPPlayState)e.newState)
            {
                //曲が終わったとき
                case WMPPlayState.wmppsMediaEnded:
                    await Task.Delay(300);  //インターバルを設けないと動かない
                    NextMusic();
                    break;

                case WMPPlayState.wmppsReady:
                    break;
            }

        }

        private void Deiri_Click(object sender, EventArgs e)
        {

        }

        private void TukiLabel_Click(object sender, EventArgs e)
        {

        }

        private void DailyLabel_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }



        private void KyokuReturn_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            KyokuReturn.Image = Image.FromFile("曲戻し_カーソル後.png");

        }

        private void KyokuReturn_MouseLeave(object sender, EventArgs e)
        {
            KyokuReturn.Image = Image.FromFile("曲戻し.png");

        }

        private void KyokuReturn_Click(object sender, EventArgs e)
        {
            KetteiOto();
            if (isPlay)
            {
                if (MusicList.Length == 0) return;

                // 再生中なら一旦止めてから巻き戻す
                Kyoku.Ctlcontrols.stop();

                // 状態を少し待つ（内部の停止処理を反映させる）
                Task.Delay(100).Wait();

                PrevMusic();
            }


        }

        private void KyokuNext_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            KyokuNext.Image = Image.FromFile("曲飛ばし_カーソル後.png");

        }

        private void KyokuNext_MouseLeave(object sender, EventArgs e)
        {
            KyokuNext.Image = Image.FromFile("曲飛ばし.png");

        }

        private void KyokuNext_Click(object sender, EventArgs e)
        {
            KetteiOto();
            if (isPlay)
            {
                if (MusicList.Length == 0) return;

                Kyoku.Ctlcontrols.stop();
                Task.Delay(100).Wait();

                NextMusic();
            }


        }

        //曲の再生するボタン↓↓↓
        private void Playback_MouseEnter(object sender, EventArgs e)
        {
            IdoOto();
            if (isPlay == true)
            {
                //再生したい
                Playback.Image = Image.FromFile("曲停止_カーソル後.png");
            }
            else
            {
                Playback.Image = Image.FromFile("曲再生_カーソル後.png");
            }
        }

        private void Playback_MouseLeave(object sender, EventArgs e)
        {
            if (isPlay == true)
            {
                //再生したい
                Playback.Image = Image.FromFile("曲停止.png");
            }
            else
            {
                Playback.Image = Image.FromFile("曲再生.png");
            }
        }

        private void Playback_Click(object sender, EventArgs e)
        {
            KetteiOto();
            if (isPlay)
            {
                // 停止処理
                Playback.Image = Image.FromFile("曲再生.png");
                isPlay = false;
                Kyoku.Ctlcontrols.stop();
            }
            else
            {
                // 再生処理
                Playback.Image = Image.FromFile("曲停止.png");
                isPlay = true;
                PlayMusic(GetMusicIndex());
            }
            CommonDB.SaveMusicSettings(isShuffle,isPlay);       //データベースに処理の保存
        }

        //飾りセリフの点滅
        private void timer3_Tick(object sender, EventArgs e)
        {
            SerihuKazari.Visible = !SerihuKazari.Visible;
        }

        private void SerihuKazari_Click(object sender, EventArgs e)
        {
            KetteiOto();
            ChangeSerihuSet();
        }

        private void Serihu_Click(object sender, EventArgs e)
        {
            KetteiOto();
            ChangeSerihuSet();
        }

        private void Tukau_tx_KeyDown(object sender, KeyEventArgs e)
        {
            //ちなみに e は「押されたキーの情報」を持っているイベント引数
            if (e.KeyCode == Keys.Enter)
            {
                // Enterキーで確定ボタンと同じ動作を実行
                Kakutei_Click(sender, e);

                // 「音」や改行を防ぐ
                e.SuppressKeyPress = true;
            }
        }

        private void Hint_M_Click(object sender, EventArgs e)
        {
            KetteiOto();
            CommonFunction.ShowInfo(
                "【ミッションについて】",
                "設定したはいいけど、なかなかタスクを達成できない時もあると思います。\n" +
                "そんなあなたには「ミッション」を達成することを目標にするのがおすすめ！\n\n" +
                "「ミッション」はノーマルとボーナスの項目から1日に1回ランダムで選ばれます！\n" +
                "ここでは普通に達成するより報酬金額が上乗せされているのが特徴です。\n\n" +
                "上乗せされる報酬金額は以下のように変わります。\n\n" +
                "・元の報酬金額が300円以下：  +200円\n\n" +
                "・元の報酬金額が400円以上600円以下：  +400円\n\n" +
                "・元の報酬金額が700円以上：  +600円\n\n" +
                "※ 達成しなくてもペナルティはありません！" ,
            this // 親フォームを渡す（モーダル表示用）
            );
        }

        private void MusicMode_Click(object sender, EventArgs e)
        {
            KetteiOto();
            if (isShuffle)
            {
                // シャッフルモード
                MusicMode.Image = Image.FromFile("曲リピート.png");
                isShuffle = false;
                if (isPlay)
                {
                    PlayMusic(GetMusicIndex());
                }
            }
            else
            {
                // 順番再生モード
                MusicMode.Image = Image.FromFile("曲シャッフル.png");
                isShuffle = true;
                if (isPlay)
                {
                    PlayMusic(GetMusicIndex());
                }
            }
            CommonDB.SaveMusicSettings(isShuffle, isPlay);       //データベースに処理の保存

        }
    }
}
