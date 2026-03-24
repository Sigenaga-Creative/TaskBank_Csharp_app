using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using static SYUUKAN.Home;

namespace SYUUKAN
{
    internal class CommonDB
    {
        //データベース接続をconnectionDBにグローバルエリアで保存
        private static string connectionDB =
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Owner\OneDrive - 神戸電子専門学校\デスクトップ\TaskBank\Home.mdf;Integrated Security=True";
        //(意味)Data Source = どこのサーバーに繋ぐか
        //AttachDbFilename = どのデータベースを開くか
        //Integrated Security=True「Windowsのログイン情報を使って接続する」要は、「ユーザー名・パスワードいらない設定」


        // 使用金額を減らす処理
        public static void HerasuMoney(int Kazu)
        {
            //SqlConnection = データべースとの接続
            //new SqlConnection(connectionDB) =　グローバルエリアで保存しておいた
            //connectionDBからデータベースの場所で新しくconnを作る
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                //接続を開く(connはデータベースとSQLをつなぐ存在)
                conn.Open();

                //queryはただの文字列。SQL文を仮代入しているだけ(@をつけることで安全にC#のものをもっていける)
                string query = "UPDATE Home SET TukaeruMoney = TukaeruMoney - @Kazu WHERE Id = 1";
                //cmdに接続場所とSQL文まとめて実行準備
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //持ってきたKazuで@Kazuに代入して使えるようにしているイメージ
                    cmd.Parameters.AddWithValue("@Kazu", Kazu);

                    //データベースで実行
                    cmd.ExecuteNonQuery();

                }

            }
        }

        // 現在の残高を調べる処理
        public static int GenMoney()
        {
            //usingは後かたずけを自動化してるイメージ。
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                //一番新しいものを取得。setuはただの文字列。SQL文を仮代入しているだけ
                string setu = "SELECT TukaeruMoney FROM Home WHERE Id = 1";

                using (SqlCommand cmd = new SqlCommand(setu, conn))
                {
                    object result = cmd.ExecuteScalar();
                    //値がある＋moneyを数字に変換して数字であるかチェック
                    if (result != null && int.TryParse(result.ToString(), out int money))
                    {
                        return money;
                    }
                    return 0;

                }//usingがあると、ここでcmdが自動破棄


            }//usingがあると、ここでconnが自動破棄(ないと残り続ける)
        }
        // 金額を更新する処理
        public static void KousinMoney(int newMoney)
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                string query = "UPDATE Home SET TukaeruMoney = @money WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@money", newMoney);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 金額を増やす処理
        public static void HuyasuMoney(int Kazu)
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                string query = "UPDATE Home SET TukaeruMoney = TukaeruMoney + @Kazu WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Kazu", Kazu);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //デイリーの達成状況の更新
        public static void TasseiDailyDB(bool TasseiDaily)
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                string query = "UPDATE Home SET KakuteiDaily = @Tassei WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Tassei", TasseiDaily);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        // デイリーの達成状態を取得
        public static bool GetDaily()
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "SELECT KakuteiDaily FROM Home WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();

                    //「値が存在していて、それがtrueならtrueを返す」「NULLまたは0ならfalseを返す」
                    if (result == DBNull.Value)
                    {
                        return false; // NULLなら未達成扱い
                    }

                    return Convert.ToBoolean(result);
                }
            }
        }
        // 日付の取得
        public static DateTime GetLastDay()
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "SELECT LastDay FROM Home WHERE Id = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                object result = cmd.ExecuteScalar();

                //DBに値があるならそれを日付に変換して返す、なければ最小日付を返す

                DateTime date;
                if (result != DBNull.Value)
                {
                    //入っている処理  (日付を渡す)                 
                    date = Convert.ToDateTime(result);
                }
                else
                {
                    //入っていない処理(初期値（0001/01/01）を渡す)
                    date = DateTime.MinValue;
                }

                return date;
            }
        }

        // 日付の更新
        public static void UpdateLastDay(DateTime date)
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "UPDATE Home SET LastDay = @date WHERE Id = 1";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.ExecuteNonQuery();
            }
        }

        // デイリーのタスク内容更新(ついでにその月やTaskLockをTrueに変えて保存)
        public static void UpdateDailyTask(string newTask)
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "UPDATE Home SET DailyTask = @task, MonthDB = @month, TaskLock = 1 WHERE Id = 1";
                //TaskLockがTrueに変わると変更できなくなる

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@task", newTask);
                    cmd.Parameters.AddWithValue("@month", DateTime.Now.Month);      //月の更新
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //現在のデイリータスクとロック状態を取得
        public static (string task, bool locked) GetDailyTask()
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "SELECT DailyTask, TaskLock FROM Home WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // 1行ずつデータを読む
                    if (reader.Read())
                    {
                        // 「DailyTask」列の内容を文字列として取り出す
                        string task = reader["DailyTask"].ToString();

                        // 「TaskLock」列の内容をbool型（true/false）に変換
                        bool locked = Convert.ToBoolean(reader["TaskLock"]);

                        // 2つの値をまとめて返す
                        return (task, locked);
                    }

                }
            }
            return ("", false);
        }


        //月が変わっていたらデイリーを変えられるように設定をリセット＋通知
        public static void CheckMonth()
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                // 現在の月を取得
                int GenMonth = DateTime.Now.Month;

                // データベースから前回の月を取得
                string selectQuery = "SELECT MonthDB FROM Home WHERE Id = 1";
                using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                {
                    object result = selectCmd.ExecuteScalar();

                    int savedMonth = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;

                    // 月が変わっていたらリセット
                    if (savedMonth != GenMonth)
                    {
                        string resetQuery = @"
                        UPDATE Home
                        SET
                            TaskLock = 0,         -- ロック解除
                            DailyTask = '',       -- 内容リセット
                            MonthDB = @month      -- 月を更新
                        WHERE Id = 1";

                        SqlCommand resetCmd = new SqlCommand(resetQuery, conn);
                        resetCmd.Parameters.AddWithValue("@month", GenMonth);
                        resetCmd.ExecuteNonQuery();

                        MessageBox.Show("新しい月になりました！今月のデイリーを設定しましょう！",
                                        "お知らせ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
            }
        }


        //デイリー、ノーマル、ボーナスの報酬を取得(CommonDB.ALLReward.GetRewardValue("NormalReward");)
        public static class ALLRewardGet
        {
            //  タイプごとに表示を切り替える
            public static int GetRewardValue(string rewardType)
            {
                int rewardValue = 0;

                using (SqlConnection con = new SqlConnection(connectionDB))
                {
                    con.Open();

                    // rewardTypeには "NormalReward" / "DailyReward" / "BonusReward" を入れる
                    string query = $"SELECT {rewardType} FROM [Option] WHERE Id = 1";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        object result = cmd.ExecuteScalar();

                        //取ってきた値がnullではなく、データベースの中身もnullないとき
                        if (result != null && result != DBNull.Value)
                        {
                            //データベースのNullはC#とは別物
                            rewardValue = Convert.ToInt32(result);
                        }
                    }
                }

                return rewardValue;
            }
        }

        //デイリー、ノーマル、ボーナスの報酬を設定

        public static void AllRewardSet(string rewardType, int rewardValue)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();

                string query = $"UPDATE [Option] SET {rewardType} = @value";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@value", rewardValue);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //ハードの報酬額を更新
        public static void HardUpdateReward(int taskId, string reward)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();

                string query = "UPDATE HardTasks SET Reward = @reward WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@reward", reward);
                    cmd.Parameters.AddWithValue("@id", taskId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Homeテーブルの音楽関連設定をまとめて取得
        public static (bool IsShuffle, bool IsMusicPlay) GetMusicSettings()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT TOP 1 IsShuffle, IsMusicPlay FROM Home"; // 1行だけ想定
                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool isShuffle = reader["IsShuffle"] != DBNull.Value && (bool)reader["IsShuffle"];
                        bool isMusicPlay = reader["IsMusicPlay"] != DBNull.Value && (bool)reader["IsMusicPlay"];
                        return (isShuffle, isMusicPlay);
                    }
                    else
                    {
                        // データがない場合のデフォルト値
                        return (false, false);
                    }
                }
            }
        }

        // Homeテーブルの音楽設定を保存
        public static void SaveMusicSettings(bool isShuffle, bool isMusicPlay)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Home SET IsShuffle = @IsShuffle, IsMusicPlay = @IsMusicPlay";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IsShuffle", isShuffle);
                    cmd.Parameters.AddWithValue("@IsMusicPlay", isMusicPlay);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Wishilistの報酬額を更新
        public static void WishUpdateReward(int taskId, string reward)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();

                string query = "UPDATE Wishlist SET Reward = @reward WHERE Id = @id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@reward", reward);
                    cmd.Parameters.AddWithValue("@id", taskId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        //ウォッシュリストのモード取得
        public static int GetWishlistMode()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT WishlistMode FROM Home WHERE Id = 1"; // 1行だけ想定
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    object result = cmd.ExecuteScalar();
                    return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                }
            }
        }

        //ウォッシュリストのモード保存
        public static void SaveWishlistMode(int mode)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Home SET WishlistMode = @mode WHERE Id = 1";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@mode", mode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //サービスの項目で最後に達成した日付の取得
        public static DateTime? GetServiceLastDay()
        {
            //DateTime?　はnullを許容している
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT ServiceLastDay FROM Home";
                SqlCommand cmd = new SqlCommand(query, con);
                object result = cmd.ExecuteScalar();

                if (result == DBNull.Value || result == null)
                {
                    return (DateTime?)null;
                }
                else
                {
                    return (DateTime?)Convert.ToDateTime(result);
                }
            }
        }

        //サービスの項目で最後に達成した日付の保存
        public static void SaveServiceLastDay(DateTime date)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Home SET ServiceLastDay = @date";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.ExecuteNonQuery();
            }
        }

        //ノーマルとボーナスからタスクと報酬金額を取得
        public static List<(string TaskName, int Reward, string Type)> GetTasks()
        {
            var tasks = new List<(string TaskName, int Reward, string Type)>();

            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();

                int normalReward = 0;
                int bonusReward = 0;

                // Optionテーブルから報酬金額を取得
                string optionQuery = "SELECT NormalReward, BonusReward FROM [Option]";
                using (SqlCommand cmd = new SqlCommand(optionQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        normalReward = Convert.ToInt32(reader["NormalReward"]);
                        bonusReward = Convert.ToInt32(reader["BonusReward"]);
                    }
                }

                // NormalTasks 取得
                string normalQuery = "SELECT TaskName FROM NormalTasks";
                using (SqlCommand cmd = new SqlCommand(normalQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["TaskName"].ToString();
                        tasks.Add((name, normalReward, "Normal"));
                    }
                }

                // BonusTasks 取得
                string bonusQuery = "SELECT TaskName FROM BonusTasks";
                using (SqlCommand cmd = new SqlCommand(bonusQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader["TaskName"].ToString();
                        tasks.Add((name, bonusReward, "Bonus"));
                    }
                }
            }

            return tasks;
        }

        //ミッションのタスク名とロック状態、報酬金額をまとめて取得
        public static ServiceTask GetServiceInfo()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT ServiceTask, ServiceLock, ServiceReward FROM Home";
                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ServiceTask
                        {
                            TaskName = reader["ServiceTask"].ToString(),
                            Lock = Convert.ToBoolean(reader["ServiceLock"]),
                            Reward = Convert.ToInt32(reader["ServiceReward"])
                        };
                    }
                }
            }

            // データが存在しない場合のデフォルト値
            return new ServiceTask
            {
                TaskName = "",
                Lock = false,
                Reward = 0
            };
        }


        //ミッションのタスク名とロック状態、報酬金額をまとめて保存
        public static void UpdateServiceInfo(string serviceTask, bool serviceLock, int serviceReward)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Home SET ServiceTask = @Task, ServiceLock = @Lock, ServiceReward = @Reward";
                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Task", (object)serviceTask ?? DBNull.Value);  //よくわかっていないけどデータベースのnullとして扱うらしい
                cmd.Parameters.AddWithValue("@Lock", serviceLock);
                cmd.Parameters.AddWithValue("@Reward", serviceReward);

                cmd.ExecuteNonQuery();
            }
        }

        //ウォッシュリストを報酬金額か使用するか切り分けるのを持ってくるやつ
        public static bool GetSiyouMode(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT SiyouMode FROM Wishlist WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                object result = cmd.ExecuteScalar();
                return result != DBNull.Value && Convert.ToBoolean(result);
            }
        }

        //ウォッシュリストのモードを保存する処理
        public static void UpdateSiyouMode(int id, bool mode)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Wishlist SET SiyouMode = @Mode WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Mode", mode);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
