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
    public partial class wishlistForm : Form
    {
        //データベース接続をconnectionDBにグローバルエリアで保存
        private static string connectionDB =
            @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Owner\OneDrive - 神戸電子専門学校\デスクトップ\TaskBank\Home.mdf;Integrated Security=True";

        private Home home;      //Home画面の変数を維持する変数
        private int Mode;
        private int LastInsertedId;

        public wishlistForm(Home _home)
        {
            InitializeComponent();
            home = _home;   //ホーム変数の保持

            Mode = CommonDB.GetWishlistMode();      //モードをデータベースから取得




            TaitoruHaikei.Image = Image.FromFile("ウォッシュリスト背景.png");

            Kakutei.Image = Image.FromFile("横確定ボタン.png");
            Hatena.Image = Image.FromFile("ハテナボタン.png");
            if (Mode == 1)
            {
                ModeButton.Image = Image.FromFile("削除モード.png");
            }
            else
            {
                ModeButton.Image = Image.FromFile("履歴モード.png");
            }

            //使用モードのチェンジ
            CommonFunction.MoziEvent(WishTask, "追加するタスクを入力してください。");   //テキストボックスの初期化
            CommonFunction.SetMozi(WishTask, "追加するタスクを入力してください。");

            CommonFunction.MoziEvent(Reward, "報酬金額");   //テキストボックスの初期化
            CommonFunction.SetMozi(Reward, "報酬金額");


            this.Click += Form_ClickOut;  // ← フォーム全体にClickイベント追加

            //パネルの設定
            WishlistRan.FlowDirection = FlowDirection.TopDown;        //パネルを縦方向にそろえる
            WishlistRan.WrapContents = false;
            WishlistRan.AutoScroll = true;                     // はみ出したらスクロール
            WishlistRan.Padding = new Padding(2);              // 内側余白
        }
        //######################################【関数エリア】###########################################

        //追加と削除、項目名が表示される関数(共通)
        private void AddTaskToForm(int id, string name, bool completed, int TaskCount, int TaskReward)
        {
            Panel panel = new Panel();
            panel.Height = 35;
            panel.Width = WishlistRan.ClientSize.Width - 62;
            panel.Margin = new Padding(5);

            // 【IDラベル】左端に表示
            Label lblId = new Label();
            lblId.Text = TaskCount.ToString();          // IDを表示
            lblId.Top = 5;
            lblId.Left = 0;
            lblId.Width = 30;
            lblId.Font = new Font("Meiryo UI", 12);
            lblId.BackColor = Color.FromArgb(230, 176, 224);
            lblId.ForeColor = Color.White;
            lblId.TextAlign = ContentAlignment.MiddleCenter;

            TextBox txtName = new TextBox();
            txtName.MaxLength = 35;
            txtName.Multiline = false;
            txtName.Text = name;
            txtName.Top = 4;
            txtName.Left = 50;
            txtName.Width = 600;
            txtName.Tag = id;
            txtName.MaxLength = 35;
            txtName.Font = new Font("Meiryo UI", 16);
            txtName.BorderStyle = BorderStyle.None;     //入力しようとすると編集モードになるイメージの処理
            txtName.ReadOnly = true;        //基本は読み込み専用にしている。
            txtName.Click += (s, e) => { txtName.ReadOnly = false; };   //クリックすると変えられるようになる

            txtName.Leave += (s, e) =>      //処理を離れると名前をデータベースに保存し、読み込み専用へ
            {
                txtName.ReadOnly = true;
                WishlistUpdateName(id, txtName.Text);
            };

            // 報酬テキストボックス
            int reward = TaskReward;
            TextBox rewardBox = new TextBox();
            rewardBox.MaxLength = 8;
            rewardBox.Multiline = false;
            rewardBox.Text = TaskReward.ToString();
            rewardBox.ForeColor = Color.White;
            rewardBox.Top = 4;
            rewardBox.Left = 660;
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
                    CommonDB.WishUpdateReward(id, rewardBox.Text);       //タスクの更新
                }
            };

            CheckBox chkSiyouMode = new CheckBox();
            chkSiyouMode.Text = "使用モード";
            chkSiyouMode.Left = 795;   // パネル内の位置調整
            chkSiyouMode.Top = 5;
            chkSiyouMode.Size = new Size(20, 30);
            chkSiyouMode.Checked = CommonDB.GetSiyouMode(id);  // 初期状態をDBから取得
            chkSiyouMode.CheckedChanged += (s, e) =>
            {
                bool isChecked = chkSiyouMode.Checked;
                // データベースに更新
                CommonDB.UpdateSiyouMode(id, isChecked);

                if (isChecked)
                {
                    rewardBox.BackColor = Color.FromArgb(254, 169, 163);
                }
                else
                {
                    rewardBox.BackColor = Color.FromArgb(163, 169, 254);
                }
            };
            bool Checked = chkSiyouMode.Checked;
            if (Checked)
            {
                rewardBox.BackColor = Color.FromArgb(254, 169, 163);
            }
            else
            {
                rewardBox.BackColor = Color.FromArgb(163, 169, 254);
            }

            Button btnComplete = new Button();
            btnComplete.Text = "達成";
            btnComplete.Left = 820;
            btnComplete.Top = 0;
            btnComplete.Font = new Font("Meiryo UI", 16);
            btnComplete.Size = new Size(80, 35);
            btnComplete.Click += (s, e) =>
            {
                // 既に達成済みなら無視
                if (completed)
                {
                    return;
                }
                TasseiTask(id);
            };        //ラムダ式：どこのidかで識別し、達成させる


            Button btnDelete = new Button();
            btnDelete.Text = "削除";
            btnDelete.Left = 910;
            btnDelete.Top = 0;
            btnDelete.Font = new Font("Meiryo UI", 16);
            btnDelete.Size = new Size(80, 35);
            btnDelete.Click += (s, e) =>
            {
                IdDelete(id);                
            };

            //達成していたなら色チェンジ
            if (completed)
            {
                panel.BackColor = Color.FromArgb(240, 240, 240);
                // テキストをグレー表示
                txtName.ForeColor = Color.Gray;
                txtName.BackColor = Color.LightGray;
                txtName.Enabled = false;
                rewardBox.ForeColor = Color.Gray;
                rewardBox.BackColor = Color.LightGray;
                rewardBox.Enabled = false;

                // ボタンを押せなくする
                btnComplete.Enabled = false;
                btnComplete.BackColor = Color.LightGray;
                btnComplete.Text = "達成済";
                btnComplete.Size = new Size(90, 35);
                btnComplete.Left = 810;

                chkSiyouMode.Left = 790;   // パネル内の位置調整
                chkSiyouMode.Enabled = false;


            }

            //パネルの準備
            panel.Controls.Add(lblId);
            panel.Controls.Add(txtName);
            panel.Controls.Add(rewardBox);
            panel.Controls.Add(chkSiyouMode);
            panel.Controls.Add(btnComplete);
            panel.Controls.Add(btnDelete);

            // パネルの表示
            WishlistRan.Controls.Add(panel);

        }

        //Wishlistのロードに必要
        private void LoadTask()
        {
            WishlistRan.Controls.Clear();     // ← 表示エリアをリセット(古いものの重複防止)
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT * FROM Wishlist ORDER BY IsCompleted ASC, Id ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();     //「SELECT文を実行」して、結果を一行ずつ
                                                                //読み取るためのオブジェクトを取得
                                                                //読み込んであればTrueを返す→ある限りループする
                int NotActiveCount = 0;     //未達成項目のカウント
                int CompletedCount = 0;     //達成済み項目のカウント
                bool LabelAdd = false;

                while (reader.Read())
                {

                    //string name = reader["WishName"].ToString();  取得したものをnameに文字化
                    string name = reader["WishName"].ToString();
                    //達成したかを取得
                    bool completed = (bool)reader["IsCompleted"];
                    int reward = Convert.ToInt32(reader["Reward"]);

                    // 未達成と達成済みで番号を分ける
                    if (completed == false)
                    {
                        NotActiveCount++;
                        AddTaskToForm((int)reader["Id"], name, completed, NotActiveCount, reward);
                    }
                    else
                    {
                        // 最初の達成済みタスクに入る直前で一度だけ区切りラベルを追加
                        if (LabelAdd == false)
                        {
                            Label separator = new Label();
                            separator.Text = "―― 達成済み ――";
                            separator.Font = new Font("Meiryo UI", 12, FontStyle.Italic);
                            separator.ForeColor = Color.Gray;
                            separator.TextAlign = ContentAlignment.MiddleCenter;
                            separator.Width = WishlistRan.ClientSize.Width -62;
                            separator.Height = 30;
                            separator.Margin = new Padding(0, 5, 0, 5);
                            WishlistRan.Controls.Add(separator);

                            LabelAdd = true;
                        }
                    }
                    if(completed == true)
                    {
                        //達成済みのものをカウント
                        CompletedCount++;
                        AddTaskToForm((int)reader["Id"], name, completed, CompletedCount, reward);
                    }
                }
            }
        }

        //タスクの追加
        private void NameInsert(string WishName)
        {
            // 現在のタスク数を取得
            int taskCount = GetTaskCount();

            // 上限チェック（100個まで）
            if (taskCount >= 100)
            {
                MessageBox.Show("タスクの最大数に到達しました。これ以上は増やせません。", "上限到達",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionDB))
            {
                conn.Open();

                string query = @"
                    INSERT INTO Wishlist (WishName)
                    OUTPUT INSERTED.Id
                    VALUES (@name)
                ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", WishName);

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
                string query = "DELETE FROM Wishlist WHERE Id = @id";
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
                string q = "SELECT COUNT(*) FROM Wishlist";
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
                    UPDATE Wishlist
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
        private void WishlistUpdateName(int id, string newName)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "UPDATE Wishlist SET WishName = @name WHERE Id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }



        //タスクの名前取得
        private string GetName(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                // 名前の取得
                string query = "SELECT WishName FROM Wishlist WHERE Id = @id";
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
            //Wishlistの報酬額を取得
            int reward = 0;
            bool SiyouMode = CommonDB.GetSiyouMode(taskId);

            //報酬金額の取得
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                con.Open();
                string query = "SELECT Reward FROM Wishlist WHERE Id = @id";
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


            string name = GetName(taskId);

            // 使用するモードだったら
            if (SiyouMode)
            {
                //減らす
                CommonDB.HerasuMoney(reward);
                // ホーム画面のセリフを変更
                home.TukauSerihuSet(name, reward);

            }
            else
            {
                //増やす
                CommonDB.HuyasuMoney(reward);
                // ホーム画面のセリフを変更
                home.HuyasuSerihuSet(name, reward);
            }

            // ホームのラベル更新
            home.GenMoney_C();


            // --- モード分岐 ---
            if (Mode == 1)
            {
                // 【削除モード】
                IdDelete(taskId);
            }
            else
            {
                // 【履歴モード】
                TrueTassei(taskId);
            }

            // 達成完了メッセージ
            MessageBox.Show($"やりたいことを成し遂げました！",
                            "【Wishlist】", MessageBoxButtons.OK, MessageBoxIcon.Information);



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

                // 0円未満チェック
                if (rewardValue < 0)
                {
                    MessageBox.Show("報酬金額は0円未満に設定できません。",
                        "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Reward.Text = "0";
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

        private void wishlistForm_Load(object sender, EventArgs e)
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
            string WishName = WishTask.Text.Trim();
            string taskReward = Reward.Text.Trim();

            // 入力チェック
            if (string.IsNullOrWhiteSpace(WishName) || WishName == "追加するタスクを入力してください。")
            {
                CommonFunction.SetMozi(WishTask, "追加するタスクを入力してください。");
                MessageBox.Show("タスク名を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NameInsert(WishName);       //追加とLastInsertedIdにIdを更新


            taskReward = CheckReward(Reward);   //報酬金額のチェック

            if (Reward.Text != "報酬金額")
            {
                CommonDB.WishUpdateReward(LastInsertedId, taskReward);  //報酬金額の更新
            }
            else
            {
                CommonDB.WishUpdateReward(LastInsertedId, "0");  //報酬金額の更新
            }


            LoadTask();       //再表示

            CommonFunction.SetMozi(Reward, "報酬金額");
            CommonFunction.SetMozi(WishTask, "追加するタスクを入力してください。");

        }

        private void WishiTask_Enter(object sender, EventArgs e)
        {
            home.KetteiOto();
            //Enterが押されると以下の処理をする
            CommonFunction.SetMozi(WishTask, "追加するタスクを入力してください。");

        }

        private void WishiTask_KeyDown(object sender, KeyEventArgs e)
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
            CommonFunction.MoziKesu(WishTask);

        }

        private void Hatena_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.ShowInfo(
                "【Wishlist】",
                " 項目ごとに金額を変えられますが、1回しか達成することはできません！\n" +
                " “ご褒美などでやりたいこと”や、“1回すればもうしないこと”などを\n" +
                "登録するのがおすすめです。\n\n" +
                "・「達成」ボタンで処理確定！\n" +
                "※　金額は最大1000万円から0円まで調整できます。\n\n" +
                "・ チェックを入れるとお小遣い使用モードへ\n" +
                "※　報酬金額のラベル：赤　使用モード / 青　報酬モード\n\n" +
                "・最大100件まで　登録可能\n" +
                "※　年のはじめにやりたいことリストとして100個更新するのもおすすめ\n\n"+
                "・「削除」ボタンでタスクを削除\n" +
                "※　一度削除したタスクは元に戻せないので注意してください。\n\n" +
                "「自動削除モードについて」\n" +
                "オンにしたまま「達成」ボタンを押すと履歴に残らずに削除されます。" ,

            this // 親フォームを渡す（モーダル表示用）
            );
        }

        private void Reward_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        private void ModeButton_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            if (Mode == 1)
            {
                ModeButton.Image = Image.FromFile("履歴モード.png");
                Mode = 0;

            }
            else
            {
                ModeButton.Image = Image.FromFile("削除モード.png");
                Mode = 1;
            }
            CommonDB.SaveWishlistMode(Mode);

        }

        private void Mozikesu_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();

        }

        private void Reward_KeyDown(object sender, KeyEventArgs e)
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


    }
}
