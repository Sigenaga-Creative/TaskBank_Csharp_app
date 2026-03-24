using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SYUUKAN
{
    public partial class Option : Form
    {
        //グローバルエリア
        public event Action<string> DailyTaskChanged;   //eventを定義(デイリーが変わったら内容を返すため)
                                                        //Action<string> : 設置した場所へ文字型を持っていく
        public event EventHandler DailyMoneyChanged;    //eventを定義(ホーム画面に金額を反映させるための通知を送る)
                                                        //EvenHandler :設置した場所へ通知を送るとき

        private Home home;      //Home画面の変数を維持する変数



        public Option(Home _home)
        {
            InitializeComponent();
            home = _home; // ホームを保持
            this.Click += Form_ClickOut;  // ← フォーム全体にClickイベント追加


            //画面を飾る
            TaitoruHaikei.Image = Image.FromFile("設定背景.png");
            Kakutei_D.Image = Image.FromFile("横確定ボタン.png");
            KakuteiMoney.Image = Image.FromFile("横確定ボタン.png");

            HomeHatena.Image = Image.FromFile("ハテナボタン.png");
            HatenaReward.Image = Image.FromFile("ハテナボタン.png");
            Hint_D.Image = Image.FromFile("ハテナボタン.png");

            //報酬テキストボックスの出力
            RewardText();


        }

        private void OptionForm_Load(object sender, EventArgs e)
        {

        }

        //######################################【関数エリア】###########################################

        //報酬金額の入力に対するエラーチェック
        private int? RewardCheck(TextBox box, string rewardType, List<string> errors)
        {
            string text = box.Text.Trim();

            // 未入力ならスキップ
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
 

            // 数値変換
            int value = int.Parse(text);

            string TypeName = "";
            switch (rewardType)
            {
                case "DailyReward":
                    TypeName = "デイリー";
                    break;
                case "NormalReward":
                    TypeName = "ノーマル";
                    break;
                case "BonusReward":
                    TypeName = "ボーナス";
                    break;

            }


            // 範囲チェック
            if (value < 100)
            {
                errors.Add($"{TypeName}が{value}円になっています。最低でも自分に100円あげましょう！");
                value =  CommonDB.ALLRewardGet.GetRewardValue(rewardType);
                //報酬テキストボックスの出力
                RewardText();

            }

            if (value > 900)
            {
                errors.Add($"{TypeName}で{value}円は少し高くないですか？900円までにしましょう！");
                value = CommonDB.ALLRewardGet.GetRewardValue(rewardType);
                //報酬テキストボックスの出力
                RewardText();

            }

            // 100の倍数チェック
            if (value % 100 != 0)
            {
                errors.Add($"{TypeName}を100円単位で入力してください。");
                value = CommonDB.ALLRewardGet.GetRewardValue(rewardType);
                //報酬テキストボックスの出力
                RewardText();

            }

            return value;
        }

        //フォームをクリックするとテキストボックス解除(テキストボックスに文字で知らせる場合必須)
        private void Form_ClickOut(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        //報酬の金額をデータベースから取ってきてテキストボックスへ出力(読み込み)
        private void RewardText()
        {
            int Daily_R = CommonDB.ALLRewardGet.GetRewardValue("DailyReward");
            int Nomal_R = CommonDB.ALLRewardGet.GetRewardValue("NormalReward");
            int Bonus_R = CommonDB.ALLRewardGet.GetRewardValue("BonusReward");

            //テキストボックスへ文字の指定
            CommonFunction.MoziEvent(DailyMoney, Daily_R.ToString());   //テキストボックスのイベント追加
            CommonFunction.SetMozi(DailyMoney, Daily_R.ToString());

            CommonFunction.MoziEvent(NormalMoney, Nomal_R.ToString());   //テキストボックスのイベント追加
            CommonFunction.SetMozi(NormalMoney, Nomal_R.ToString());

            CommonFunction.MoziEvent(BonusMoney, Bonus_R.ToString());   //テキストボックスのイベント追加
            CommonFunction.SetMozi(BonusMoney, Bonus_R.ToString());


        }

        //報酬金額をチェックしエラーか保存を返す
        private void RewardUpdate()
        {


            // エラーメッセージをためるリスト
            List<string> errors = new List<string>();

            //データベースに報酬額を設定
            try
            {
                //保存できる数字かチェック
                int? dailyReward = RewardCheck(DailyMoney, "DailyReward", errors);
                int? normalReward = RewardCheck(NormalMoney, "NormalReward", errors);
                int? bonusReward = RewardCheck(BonusMoney, "BonusReward", errors);

                // エラーがあればまとめて表示(念のため)
                if (errors.Count > 0)
                {
                    string message = string.Join("\n", errors);
                    MessageBox.Show(message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // 保存処理は行わない
                }

                // 入力があるものだけ保存
                if (dailyReward.HasValue)
                    CommonDB.AllRewardSet("DailyReward", dailyReward.Value);
                if (normalReward.HasValue)
                    CommonDB.AllRewardSet("NormalReward", normalReward.Value);
                if (bonusReward.HasValue)
                    CommonDB.AllRewardSet("BonusReward", bonusReward.Value);



                //報酬テキストボックスの出力
                RewardText();

            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました:\n{ex.Message}");
            }

        }

        //##############################################################################################

        private void Kakutei_D_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();
            Kakutei_D.Image = Image.FromFile("横確定ボタン_選択後.png");
        }

        private void Kakutei_D_MouseLeave(object sender, EventArgs e)
        {
            Kakutei_D.Image = Image.FromFile("横確定ボタン.png");
        }

        private void Kakutei_D_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            string newTask = DailyTaskTextBox.Text.Trim();

            //なにも入力されてなかったらエラー表示
            if (string.IsNullOrEmpty(newTask))
            {
                MessageBox.Show("デイリーの内容を入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
          
            var dailyInfo = CommonDB.GetDailyTask();    //ちなみにvarは自動で型を用意してくれる。型が決まっているとき限定
            bool isLocked = dailyInfo.locked;
            bool PMoney = false;

            string message;
            if (isLocked == true)
            {
                message = "まだ月が変わっていません。\nこの操作をするとお小遣いが「1000円」減ります。\nそれでも変更しますか？";
                PMoney = true;
            }
            else
            {
                message = "デイリーを決めると来月まで変更できません。\n変更するにはお小遣い「1000円」が必要です。\n【この内容で登録しますか？】";
            }

            DialogResult result = MessageBox.Show(
                message,
                "【注意】",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            //上のダイアログでyesを押した場合
            if (result == DialogResult.Yes)
            {
                // お金を減らす処理（ロック中の場合のみ）
                if (PMoney == true)
                {
                    CommonDB.HerasuMoney(1000);
                }
                CommonDB.UpdateDailyTask(newTask);
                MessageBox.Show("デイリータスクを更新しました！", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                //入力されたものをホームへ持っていく処理
                DailyTaskChanged?.Invoke(newTask); ;
                DailyMoneyChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        //##############################################################################################

        //報酬金額を確定するボタンの処理
        private void KakuteiMoney_MouseEnter_1(object sender, EventArgs e)
        {
            home.IdoOto();
            KakuteiMoney.Image = Image.FromFile("横確定ボタン_選択後.png");
        }

        private void KakuteiMoney_MouseLeave(object sender, EventArgs e)
        {
            KakuteiMoney.Image = Image.FromFile("横確定ボタン.png");
        }

        private void KakuteiMoney_Click(object sender, EventArgs e)
        {
            home.KetteiOto();


            // エラーメッセージをためるリスト
            List<string> errors = new List<string>();

            //データベースに報酬額を設定
            try
            {
                //できる数字かチェック
                int? dailyReward = RewardCheck(DailyMoney, "DailyReward", errors);
                int? normalReward = RewardCheck(NormalMoney, "NormalReward", errors);
                int? bonusReward = RewardCheck(BonusMoney, "BonusReward", errors);

                // エラーがあればまとめて表示
                if (errors.Count > 0)
                {
                    string message = string.Join("\n", errors);
                    MessageBox.Show(message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // 保存処理は行わない
                }

                // 入力があるものだけ保存
                if (dailyReward.HasValue)
                    CommonDB.AllRewardSet("DailyReward", dailyReward.Value);
                if (normalReward.HasValue)
                    CommonDB.AllRewardSet("NormalReward", normalReward.Value);
                if (bonusReward.HasValue)
                    CommonDB.AllRewardSet("BonusReward", bonusReward.Value);

                MessageBox.Show("報酬金額が保存できました！", "完了", MessageBoxButtons.OK, MessageBoxIcon.Information);


                //報酬テキストボックスの出力
                RewardText();

            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました:\n{ex.Message}");
            }
            
        }

        //###############################################################


        //↓↓↓テキストボックスを数字以外の入力を受け付けない処理をしている
        private void DailyMoney_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        private void NormalMoney_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        private void BonusMoney_KeyPress(object sender, KeyPressEventArgs e)
        {
            CommonFunction.SugiSp_Cheak(sender, e);
        }

        //###############################################################


        //↓↓↓必要なくなった
        private void DailyMoney_Enter(object sender, EventArgs e)
        {

        }

        private void NormalMoney_Enter(object sender, EventArgs e)
        {
        }

        private void BonusMoney_Enter(object sender, EventArgs e)
        {
        }

        //###############################################################


        //開いたときにテキストボックスに選択されるのを防ぐ
        private void OptionForm_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        //デイリーテキストボックスの設定
        private void DailyTaskTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //e は「押されたキーの情報」を持っているイベント引数
            if (e.KeyCode == Keys.Enter)
            {
                // Enterキーで確定ボタンと同じ動作を実行
                Kakutei_D_Click(sender, e);

                // 「音」や改行を防ぐ
                e.SuppressKeyPress = true;
            }
        }

        private void Mozikesu_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonFunction.MoziKesu(DailyTaskTextBox);


        }
        //###############################################################

        //↓↓↓それぞれの報酬モードへ切り替え
        private void Tyokin_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonDB.AllRewardSet("DailyReward", 100);
            CommonDB.AllRewardSet("NormalReward", 100);
            CommonDB.AllRewardSet("BonusReward", 100);

            //報酬テキストボックスの出力
            RewardText();

        }

        private void Kadai_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonDB.AllRewardSet("DailyReward", 200);
            CommonDB.AllRewardSet("NormalReward", 500);
            CommonDB.AllRewardSet("BonusReward", 500);

            //報酬テキストボックスの出力
            RewardText();

        }

        private void Hugou_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonDB.AllRewardSet("DailyReward", 500);
            CommonDB.AllRewardSet("NormalReward", 900);
            CommonDB.AllRewardSet("BonusReward", 800);

            //報酬テキストボックスの出力
            RewardText();
        }

        private void Shouyo_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonDB.AllRewardSet("DailyReward", 100);
            CommonDB.AllRewardSet("NormalReward", 300);
            CommonDB.AllRewardSet("BonusReward", 600);

            //報酬テキストボックスの出力
            RewardText();
        }

        //###############################################################


        //ヒントの表示
        private void HatenaReward_Click(object sender, EventArgs e)
        {
            home.KetteiOto();

            CommonFunction.ShowInfo(
                "【報酬金額の変更について】",
                " デイリー、ノーマル、ボーナスの報酬金額を変更できます！\n" +
                "どれぐらいの金額がいいかわからなかったら自分に合いそうなモードを選択しましょう！\n" +
                "※必要であれば個別に変更することもできます。\n\n" +
                "・最低設定金額：100円\n" +
                "・最高設定金額：900円\n\n" +
                "より自由な金額を設定したい場合はハードやWishlistに登録するのがおすすめです。\n\n" +
                "※月1のみ変更可能にするかは検討中",
            this // 親フォームを渡す（モーダル表示用）
            );
        }

        //↓↓↓それぞれの報酬を保存

        private void DailyMoney_Leave(object sender, EventArgs e)
        {
            home.KetteiOto();
            //報酬金額の保存かエラー処理
            RewardUpdate();
        }

        private void NormalMoney_Leave(object sender, EventArgs e)
        {
            home.KetteiOto();

            //報酬金額の保存かエラー処理
            RewardUpdate();
        }

        private void BonusMoney_Leave(object sender, EventArgs e)
        {
            home.KetteiOto();

            //報酬金額の保存かエラー処理
            RewardUpdate();
        }

        private void DailyMoney_KeyDown(object sender, KeyEventArgs e)
        {
            // エンターを押すと次へ選択がいく処理
            if (e.KeyCode == Keys.Enter)
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void NormalMoney_KeyDown(object sender, KeyEventArgs e)
        {
            // エンターを押すと次へ選択がいく処理
            if (e.KeyCode == Keys.Enter)
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BonusMoney_KeyDown(object sender, KeyEventArgs e)
        {
            // エンターを押すと次へ選択がいく処理
            if (e.KeyCode == Keys.Enter)
            {
                this.SelectNextControl((Control)sender, true, true, true, true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void HomeHatena_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.ShowInfo(
                "【TaskBank】",
                "「TaskBankについて」\n\n" +
                "日々増え続けて手が付けられないタスク、いつの間にかなくなるお金...。\n" +
                "こんなことありませんか？\n\n\n" +
                "TaskBankはそんな人たちの手助けになるべく開発されました。\n\n" +
                "自分で予め決めていた「タスク」を達成すると、\n" +
                "報酬として使用していい「お小遣い」が増えます。\n\n" +
                "もちろん、これを守るかどうかは全てあなた次第です。\n\n" +
                "このルールを守り続けられたなら、きっと富と人間性が向上していることでしょう。\n\n" ,
            this // 親フォームを渡す（モーダル表示用）
            );
        }

        private void Tyokin_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();
        }

        private void Shouyo_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();
        }

        private void Kadai_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();
        }

        private void Hugou_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();

        }

        private void Mozikesu_MouseEnter(object sender, EventArgs e)
        {
            home.IdoOto();

        }

        private void Hint_D_Click(object sender, EventArgs e)
        {
            home.KetteiOto();
            CommonFunction.ShowInfo(
                "【デイリーについて】",
                "デイリーでは日々継続したいことを設定するのがおすすめです！\n" +
                "達成できなかったらペナルティがあるので、気をつけてください！\n\n" +
                "・ペナルティの計算：\n" +
                "(達成できなかった日数×デイリー金額) + ログインしなかった日数のペナルティ(4日以上で最大に到達)\n\n" +
                "・タスクは月に一度だけ変更できます。\n" +
                "※ どうしても変えたい場合はお小遣いを1000円使えば変えられます。\n\n\n" +
                "「どうして10分以内がいいの？」\n\n" +
                "人が毎日継続するにはできるだけタスクのハードルを\n" +
                "下げないといけません。10分以上かかるものだと、\n" +
                "継続できなくなる確率が上がってしまいます。\n\n" +
                "※ すでに継続できているものなら10分以上にしても問題ありません。\n" ,
            this // 親フォームを渡す（モーダル表示用）
            );
        }
    }
}
