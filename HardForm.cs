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
    public partial class HardForm : Form
    {
        //データベース接続をconnectionDBにグローバルエリアで保存
        private static string connectionDB =
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Owner\OneDrive - 神戸電子専門学校\デスクトップ\TaskBank\Home.mdf;Integrated Security=True";

        private Home home;      //Home画面の変数を維持する変数
        private int LastInsertedId;


        public HardForm(Home _home)
        {
            InitializeComponent();
            home = _home; // ホームフォームを保持

            TaitoruHaikei.Image = Image.FromFile("ハード背景.png");
            Kakutei.Image = Image.FromFile("横確定ボタン.png");
            Hatena.Image = Image.FromFile("ハテナボタン.png");

            CommonFunction.MoziEvent(HardTask, "追加するタスクを入力してください。");   //テキストボックスの初期化
            CommonFunction.SetMozi(HardTask, "追加するタスクを入力してください。");

            CommonFunction.MoziEvent(Reward, "報酬金額");   //テキストボックスの初期化
            CommonFunction.SetMozi(Reward, "報酬金額");


            this.Click += Form_ClickOut;  // ← フォーム全体にClickイベント追加

            //パネルの設定
            HardTaskRan.FlowDirection = FlowDirection.TopDown;        //パネルを縦方向にそろえる
            HardTaskRan.WrapContents = false;
            HardTaskRan.AutoScroll = true;                     // はみ出したらスクロール
            HardTaskRan.Padding = new Padding(2);              // 内側余白



        }
        //######################################【関数エリア】###########################################

        //追加と削除、項目名が表示される関数(共通)
        private void AddTaskToForm(int id, string name, bool completed, int TaskCount,int TaskReward)
        {
            Panel panel = new Panel();
            panel.Height = 35;
            panel.Width = HardTaskRan.ClientSize.Width - 30;
            panel.Margin = new Padding(5);

            // 【IDラベル】左端に表示
            Label lblId = new Label();
            lblId.Text = TaskCount.ToString();          // IDを表示
            lblId.Top = 6;
            lblId.Left = 0;
            lblId.Width = 30;
            lblId.Font = new Font("Meiryo UI", 12);
            lblId.BackColor = Color.FromArgb(253, 176, 157);
            lblId.ForeColor = Color.White;
            lblId.TextAlign = ContentAlignment.MiddleCenter;

            TextBox txtName = new TextBox();
            txtName.MaxLength = 35;
            txtName.Multiline = false;
            txtName.Text = name;
            txtName.Top = 5;
            txtName.Left = 50;
            txtName.Width = 630;
            txtName.Tag = id;
            txtName.MaxLength = 35;
            txtName.Font = new Font("Meiryo UI", 16);
            txtName.BorderStyle = BorderStyle.None;     //入力しようとすると編集モードになるイメージの処理
            txtName.ReadOnly = true;        //基本は読み込み専用にしている。
            txtName.Click += (s, e) => { txtName.ReadOnly = false; };   //クリックすると変えられるようになる

            txtName.Leave += (s, e) =>      //処理を離れると名前をデータベースに保存し、読み込み専用へ
            {
                txtName.ReadOnly = true;
                HardUpdateName(id, txtName.Text);
            };

            // 報酬テキストボックス
            int reward = TaskReward;
            TextBox rewardBox = new TextBox();
            rewardBox.MaxLength = 8;
            rewardBox.Multiline = false;
            rewardBox.Text = TaskReward.ToString();
            rewardBox.Top = 5;
            rewardBox.Left = 690;
            rewardBox.Width = 120;
            rewardBox.MaxLength = 8;
            rewardBox.Font = new Font("Meiryo UI", 16);
            rewardBox.ReadOnly = true;
            rewardBox.BorderStyle = BorderStyle.None;     //入力しようとすると編集モードになるイメージの処理
            rewardBox.Click += (s, e) => { rewardBox.ReadOnly = false; };   //クリックすると変えられるようになる
            // 入力制限を登録（数字のみ）
            rewardBox.KeyPress += CommonFunction.SugiSp_Cheak;  //数字以外の入力の無効化
            rewardBox.Leave += (s, e) =>      //処理を離れると値をデータベースに保存し、読み込み専用へ
            {
                rewardBox.ReadOnly = true;

                rewardBox.Text = CheckReward(rewardBox);       //報酬金額のチェック

                // 数字であり、かつ空でない場合のみ保存
                if (int.TryParse(rewardBox.Text, out int rewardValue))
                {
                    CommonDB.HardUpdateReward(id, rewardBox.Text);       //タスクの更新
                }
            };

            Button btnComplete = new Button();
            btnComplete.Text = "達成";
            btnComplete.Left = 820;
            btnComplete.Top = 1;
            btnComplete.Font = new Font("Meiryo UI", 16);
            btnComplete.Size = new Size(80, 35);
            btnComplete.Click += (s, e) => 
            { 
                TasseiTask(id); 
            };        //ラムダ式：どこのidかで識別し、達成させる


            Button btnDelete = new Button();
            btnDelete.Text = "削除";
            btnDelete.Left = 910;
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
            panel.Controls.Add(rewardBox);
            panel.Controls.Add(btnComplete);
            panel.Controls.Add(btnDelete);

            // パネルの表示
            HardTaskRan.Controls.Add(panel);

        }

        //ハードのロードに必要
        private void LoadTask()
        {
            HardTaskRan.Controls.Clear();     // ← 表示エリアをリセット(古いものの重複防止)
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT * FROM HardTasks ORDER BY Id";
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
                    int reward = Convert.ToInt32(reader["Reward"]);

                    AddTaskToForm((int)reader["Id"], name, completed, TaskCount,reward);

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

                string query = @"
                    INSERT HardTasks (TaskName)
                    OUTPUT INSERTED.Id
                    VALUES (@name)
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", taskName);

                // INSERT と同時に新しい ID を取得
                LastInsertedId = (int)cmd.ExecuteScalar();
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
                string query = "DELETE FROM HardTasks WHERE Id = @id";
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
                string q = "SELECT COUNT(*) FROM HardTasks";
                using (SqlCommand cmd = new SqlCommand(q, conn))
                {
                    object o = cmd.ExecuteScalar();

                    //三項演算子：「条件 ? 真のとき : 偽のとき」
                    return (o != null && o != DBNull.Value) ? Convert.ToInt32(o) : 0;
                }
            }
        }

        //達成をTrueへ(ハードは重要じゃない)
        private void TrueTassei(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = @"
                    UPDATE HardTasks
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
        private void HardUpdateName(int id, string newName)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE HardTasks SET TaskName = @name WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        //IDの取得     
        private int GetId(string nameId)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT Id FROM HardTasks WHERE TaskName = @Name";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", nameId);

                    object result = cmd.ExecuteScalar();  // ← 1件だけ結果を取得
                    if (result != null && int.TryParse(result.ToString(), out int id))
                    {
                        return id;
                    }
                    else
                    {
                        return -1; // 見つからなかった場合
                    }
                }
            }
        }

        //タスクの名前取得
        private string GetName(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                // 名前の取得
                string query = "SELECT TaskName FROM HardTasks WHERE Id = @id";
                string name;
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    object name_a = cmd.ExecuteScalar();
                    name = name_a?.ToString(); // null条件演算子：nullがあればnull、なければTostring
                }
                return name;
            }
        }

        //フォームをクリックするとテキストボックス解除(テキストボックスに文字で知らせる場合必須)
        private void Form_ClickOut(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        // 達成処理
        private void TasseiTask(int taskId)
        {
            //ハードの報酬額を取得
            int reward = 0;

            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT Reward FROM HardTasks WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@id", taskId);

                object result = cmd.ExecuteScalar();
                //いつも通りnullでないか＋DBもnullでないかチェックしている
                if (result != null && result != DBNull.Value)
                {
                    //問題なければ報酬金額として扱う
                    reward = Convert.ToInt32(result);
                }
            }


            // DBに加算
            CommonDB.HuyasuMoney(reward);

            // ホームのラベル更新
            home.GenMoney_C();

            // タスクの達成状態も更新(ハードはなくても良い処理)
            TrueTassei(taskId);

            string name = GetName(taskId);
            // 達成完了メッセージ
            MessageBox.Show($"タスクを達成しました！",
                            "【ハード】", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // ホーム画面のセリフを変更
            home.HuyasuSerihuSet(name, reward);


        }

        //報酬金額のチェックしその値を文字列で返す
        private string CheckReward(TextBox Reward)
        {
            try
            {
                // 空文字か文字が報酬金額なら何もしない
                if (string.IsNullOrWhiteSpace(Reward.Text) || Reward.Text == "報酬金額")
                {
                    return Reward.Text;
                }

                int rewardValue = int.Parse(Reward.Text);

                // 100の倍数チェック
                if (rewardValue % 100 != 0)
                {
                    MessageBox.Show("報酬金額は100円単位で設定してください。",
                        "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // 一番近い100円単位に丸める
                    rewardValue = (rewardValue / 100) * 100;
                    Reward.Text = rewardValue.ToString();
                }

                // 1000円未満チェック
                if (rewardValue < 1000)
                {
                    MessageBox.Show("報酬金額は1000円未満に設定できません。",
                        "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Reward.Text = "1000";
                }

                // 上限チェック
                else if (rewardValue > 10000000)
                {
                    MessageBox.Show("報酬金額の上限は1000万円です。",
                        "上限超過", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Reward.Text = "10000000";
                }


                return Reward.Text;
            }
            catch (FormatException)
            {
                MessageBox.Show("報酬金額には数字を入力してください。", "入力エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Reward.Text = "";
                return Reward.Text;
            }
        }

        //###############################################################

        private void HardForm_Load(object sender, EventArgs e)
        {
            LoadTask();
        }

        private void Kakutei_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();
            Kakutei.Image = Image.FromFile("横確定ボタン_選択後.png");

        }

        private void Kakutei_MouseLeave(object sender, EventArgs e)
        {
            Kakutei.Image = Image.FromFile("横確定ボタン.png");

        }

        private void Kakutei_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            string taskName = HardTask.Text.Trim();
            string taskReward = Reward.Text.Trim();

            // 入力チェック
            if (string.IsNullOrWhiteSpace(taskName) || taskName == "追加するタスクを入力してください。")
            {
                CommonFunction.SetMozi(HardTask, "追加するタスクを入力してください。");
                MessageBox.Show("タスク名を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NameInsert(taskName);       //追加

            taskReward = CheckReward(Reward);   //報酬金額のチェック

            if (Reward.Text != "報酬金額")
            {
                CommonDB.HardUpdateReward(LastInsertedId, taskReward);  //報酬金額の更新
            }
            else
            {
                CommonDB.HardUpdateReward(LastInsertedId, "1000");  //報酬金額の更新
            }


            LoadTask();       //再表示

            CommonFunction.SetMozi(Reward, "報酬金額");
            CommonFunction.SetMozi(HardTask, "追加するタスクを入力してください。");

        }

        private void HardTask_MouseEnter(object sender, EventArgs e)
        {

        }//間違えた

        private void HardTask_Enter(object sender, EventArgs e)
        {
            home.KetteiOto();
            //Enterが押されると以下の処理をする
            CommonFunction.SetMozi(HardTask, "追加するタスクを入力してください。");
        }

        //Enterを押すと確定ボタンを押したのと同じ動作をする
        private void HardTask_KeyDown(object sender, KeyEventArgs e)
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

        private void Mozikesu_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.MoziKesu(HardTask);
        }

        private void Hatena_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.ShowInfo(
                "【ハード】",
                " 報酬金額を項目ごとに設定できます！\n" +
                " “簡単に達成できないこと”や、“1000円以上の報酬に設定したいもの”などを\n登録するのがおすすめです。\n\n" +
                "・「達成」ボタンでお小遣いGET！\n" +
                "※　報酬金額は最大1000万円から1000円まで調整できます。\n\n" +
                "・「削除」ボタンでタスクを削除\n" +
                "※　一度削除したタスクは元に戻せないので注意してください。\n\n" +
                "・最大20件まで　登録可能",
            this // 親フォームを渡す（モーダル表示用）
            );

        }

        private void Reward_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        private void Mozikesu_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();

        }

        private void Reward_KeyDown(object sender, KeyEventArgs e)
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
    }
}
