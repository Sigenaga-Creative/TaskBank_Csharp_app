using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SYUUKAN
{
    public partial class BonusForm : Form
    {
        //データベース接続をconnectionDBにグローバルエリアで保存
        private static string connectionDB =
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Owner\OneDrive - 神戸電子専門学校\デスクトップ\TaskBank\Home.mdf;Integrated Security=True";
        private Home home;      //Home画面の変数を維持する変数


        public BonusForm(Home _home)
        {
            InitializeComponent();
            home = _home; // ホームを保持

            TaitoruHaikei.Image = Image.FromFile("ボーナス背景.png");
            Kakutei.Image = Image.FromFile("横確定ボタン.png");
            Hatena.Image = Image.FromFile("ハテナボタン.png");
            CommonFunction.MoziEvent(BonusTask, "追加するタスクを入力してください。");   //テキストボックスのイベント追加
            CommonFunction.SetMozi(BonusTask, "追加するタスクを入力してください。");    //テキストボックスのリセット
            this.ActiveControl = null;      //テキストボックスに選択されるのを防ぐ

            //パネルの設定
            BonusTaskRan.FlowDirection = FlowDirection.TopDown;        //パネルを縦方向にそろえる
            BonusTaskRan.WrapContents = false;
            BonusTaskRan.AutoScroll = true;                     // はみ出したらスクロール
            BonusTaskRan.Padding = new Padding(2);              // 内側余白
        }
        //######################################【関数エリア】###########################################

        //追加と削除、項目名が表示される関数(共通)
        private void AddTaskToForm(int id, string name, bool completed, int TaskCount)
        {
            Panel panel = new Panel();
            panel.Height = 35;
            panel.Width = BonusTaskRan.ClientSize.Width - 30;
            panel.Margin = new Padding(5);

            // 【IDラベル】左端に表示
            Label lblId = new Label();
            lblId.Text = TaskCount.ToString();          // IDを表示
            lblId.Top = 6;
            lblId.Left = 0;
            lblId.Width = 30;
            lblId.BackColor = Color.FromArgb(226, 217, 135);
            lblId.ForeColor = Color.White;
            lblId.Font = new Font("Meiryo UI", 12);
            lblId.TextAlign = ContentAlignment.MiddleCenter;

            TextBox txtName = new TextBox();
            txtName.Multiline = false;
            txtName.Text = name;
            txtName.Top = 5;
            txtName.Left = 50;
            txtName.Width = 770;
            txtName.Tag = id;
            txtName.MaxLength = 43;
            txtName.Font = new Font("Meiryo UI", 16);
            txtName.BorderStyle = BorderStyle.None;     //入力しようとすると編集モードになるイメージの処理
            txtName.ReadOnly = true;        //基本は読み込み専用にしている。
            txtName.Click += (s, e) => { txtName.ReadOnly = false; };   //クリックすると変えられるようになる

            txtName.Leave += (s, e) =>      //処理を離れると名前をデータベースに保存し、読み込み専用へ
            {
                txtName.ReadOnly = true;
                BonusUpdateName(id, txtName.Text);
            };

            Button btnComplete = new Button();
            btnComplete.Text = "達成";
            btnComplete.Left = 840;
            btnComplete.Top = 1;
            btnComplete.Font = new Font("Meiryo UI", 16);
            btnComplete.Size = new Size(80, 35);
            btnComplete.Click += (s, e) => { TasseiTask(id); };        //ラムダ式：どこのidかで識別し、達成させる


            Button btnDelete = new Button();
            btnDelete.Text = "削除";
            btnDelete.Left = 923;
            btnDelete.Top = 1;
            btnDelete.Font = new Font("Meiryo UI", 16);
            btnDelete.Size = new Size(80, 35);
            btnDelete.Click += (s, e) =>
            {
                IdDelete(id);

            };

            //パネルの準備
            panel.Controls.Add(lblId);
            panel.Controls.Add(txtName);
            panel.Controls.Add(btnComplete);
            panel.Controls.Add(btnDelete);

            // パネルの表示
            BonusTaskRan.Controls.Add(panel);

        }

        //ボーナスのロードに必要
        private void LoadTask()
        {
            BonusTaskRan.Controls.Clear();     // ← 表示エリアをリセット(古いものの重複防止)
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT * FROM BonusTasks ORDER BY Id";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();     //「SELECT文を実行」して、結果を一行ずつ
                                                                //読み取るためのオブジェクトを取得
                                                                //読み込んであればTrueを返す→ある限りループする
                int TaskCount = 0;
                while (reader.Read())
                {
                    TaskCount++;
                    //string name = reader["TaskName"].ToString();  取得したものをnameに文字化
                    string name = reader["TaskName"].ToString();
                    //達成したかを取得
                    bool completed = (bool)reader["IsCompleted"];

                    AddTaskToForm((int)reader["Id"], name, completed, TaskCount);

                }
            }
        }

        //タスクの追加
        private void NameInsert(string taskName)
        {
            // 現在のタスク数を取得
            int taskCount = GetTaskCount();

            // 上限チェック（20個まで）
            if (taskCount >= 20)
            {
                MessageBox.Show("タスクの最大数に到達しました。これ以上は増やせません。", "上限到達",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string query = "Insert INTO BonusTasks (TaskName) VALUES (@name)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", taskName);
                cmd.ExecuteNonQuery();
            }


            LoadTask(); // 更新
            this.ActiveControl = null;      //テキストボックスに選択されるのを防ぐ


        }

        //タスクの削除(厳密にいえばidを削除している)
        private void IdDelete(int id)
        {
            // 現在のタスク数を確認
            int taskCount = GetTaskCount();
            if (taskCount <= 1)
            {
                MessageBox.Show("タスクは最低1つ残す必要があります。", "削除不可",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "DELETE FROM BonusTasks WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadTask();
        }

        // DBからタスク数を取得する関数
        private int GetTaskCount()
        {
            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();
                string q = "SELECT COUNT(*) FROM BonusTasks";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    object o = cmd.ExecuteScalar();

                    //三項演算子：「条件 ? 真のとき : 偽のとき」
                    return (o != null && o != DBNull.Value) ? Convert.ToInt32(o) : 0;
                }
            }
        }

        //達成をTrueへ
        private void TrueTassei(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = @"
                    UPDATE BonusTasks
                    SET IsCompleted = 1,
                        LastDay = GETDATE()
                    WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            LoadTask();
        }

        //タスクの名前更新
        private void BonusUpdateName(int id, string newName)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE BonusTasks SET TaskName = @name WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }


        // 達成処理
        private void TasseiTask(int taskId)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();

                // 最終達成日を取得
                string checkQuery = "SELECT LastDay FROM BonusTasks WHERE Id = @id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@id", taskId);
                object result = checkCmd.ExecuteScalar();

                // 名前の取得
                string query = "SELECT TaskName FROM BonusTasks WHERE Id = @id";
                string name;
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);

                    object name_a = cmd.ExecuteScalar();
                    name = name_a?.ToString(); // null条件演算子：nullがあればnull、なければTostring
                }

                // すでに今日達成している場合はスキップ
                if (result != null && result != DBNull.Value)
                {
                    DateTime lastDay = Convert.ToDateTime(result);
                    if (lastDay.Date == DateTime.Today)
                    {
                        MessageBox.Show("このタスクは本日すでに達成済みです。",
                                        "達成済み", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                //ボーナスの報酬額を取得
                int reward = CommonDB.ALLRewardGet.GetRewardValue("BonusReward");

                // DBに加算
                CommonDB.HuyasuMoney(reward);

                // ホームのラベル更新
                home.GenMoney_C();

                // タスクの達成状態も更新
                TrueTassei(taskId);

                // 最終達成日を今日に更新
                string updateQuery = "UPDATE BonusTasks SET LastDay = @today WHERE Id = @id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@today", DateTime.Today);
                updateCmd.Parameters.AddWithValue("@id", taskId);
                updateCmd.ExecuteNonQuery();

                // 達成完了メッセージ
                MessageBox.Show($"タスクを達成しました！",
                                "【ボーナス】", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // ホーム画面のセリフを変更
                home.HuyasuSerihuSet(name, reward);
                
            }
        }

        //###############################################################

        private void BonasuForm_Load(object sender, EventArgs e)
        {
            LoadTask();
        }

        private void BonasuForm_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            this.ActiveControl = null;      //テキストボックスに選択されるのを防ぐ

        }

        private void Mozikesu_Click(object sender, EventArgs e)
        {
            home.IdoOto();
            CommonFunction.MoziKesu(BonusTask);


        }

        private void BonasuForm_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;      //テキストボックスに選択されるのを防ぐ
        }

        private void Kakutei_MouseEnter(object sender, EventArgs e)
        {
            home.KetteiOto();
            Kakutei.Image = Image.FromFile("横確定ボタン_選択後.png");
        }

        private void Kakutei_MouseLeave(object sender, EventArgs e)
        {
            Kakutei.Image = Image.FromFile("横確定ボタン.png");
        }

        private void Kakutei_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            string taskName = BonusTask.Text.Trim();

            // 入力チェック
            if (string.IsNullOrWhiteSpace(taskName) || taskName == "追加するタスクを入力してください。")
            {
                CommonFunction.SetMozi(BonusTask, "追加するタスクを入力してください。");
                MessageBox.Show("タスク名を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NameInsert(taskName);


            LoadTask();       //再表示

            CommonFunction.SetMozi(BonusTask, "追加するタスクを入力してください。");


        }

        private void BonusTask_Enter(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.SetMozi(BonusTask, "追加するタスクを入力してください。");

        }

        private void BonusTask_KeyDown(object sender, KeyEventArgs e)
        {
            //e は「押されたキーの情報」を持っているイベント引数
            if (e.KeyCode == Keys.Enter)
            {
                // Enterキーで確定ボタンと同じ動作を実行
                Kakutei_Click(sender, e);

                // 「音」や改行を防ぐ
                e.SuppressKeyPress = true;
            }
        }

        //ヒントの表示
        private void Hatena_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.ShowInfo(
                "【ボーナス】",
                "ノーマルとほぼ一緒ですが、項目ごとに1日1回しか「達成」ボタンを押すことができません。\n" +
                "“続けたい趣味”や“ノーマルに入れるほどではないけど、できたら嬉しいこと”などを登録するのがおすすめです！\n\n" +
                "・「達成」ボタンでお小遣いGET！\n" +
                "※　報酬金額は設定画面から変更できます。\n\n" +
                "・「削除」ボタンでタスクを削除\n" +
                "※　一度削除したタスクは元に戻せないので注意してください。\n\n" +
                "・最大20件まで　登録可能",
            this // 親フォームを渡す（モーダル表示用）
            );
        }

        private void Mozikesu_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();

        }
    }
}
