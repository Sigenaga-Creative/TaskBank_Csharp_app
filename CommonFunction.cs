using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SYUUKAN;

namespace SYUUKAN
{
    internal class CommonFunction
    {
        //実質数字以外の入力を受け付けなくする
        public static void SugiSp_Cheak(object sender, KeyPressEventArgs e)
        {
            // 数字とBackspaceだけを許可
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // 入力を無効化
            }
            //char.IsControl() :スペースやタブの認識
            //char.IsDigit() : 数字の認識
        }

        //テキストボックスに文字をはったり、色を変える処理
        public static void SetMozi(TextBox txt, string Mozi)
        {
            txt.Text = Mozi;
            txt.ForeColor = Color.Silver;
        }

        // テキストボックスのEnter/Leaveイベントを最初に一度だけ登録する処理
        public static void MoziEvent(TextBox txt, string Mozi)
        {
            //エンターの場合文字消す＋色を黒へ
            txt.Enter += (s, e) =>
            {
                if (txt.Text == Mozi)
                {
                    txt.Text = "";
                    txt.ForeColor = Color.Black;
                }
            };

            //離れた場合、指定文字表示＋色をシルバーへ
            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = Mozi;
                    txt.ForeColor = Color.Silver;
                }
            };
        }
        // ヒントフォームの表示(モーダル)
        public static void ShowInfo(string title, string message, Form parent = null)
        {
            // 新しいフォームを作成
            Form info = new Form();
            info.Text = title;
            info.Size = new Size(650, 650);
            info.StartPosition = FormStartPosition.CenterParent;
            info.FormBorderStyle = FormBorderStyle.FixedDialog;
            info.MaximizeBox = false;
            info.MinimizeBox = false;
            info.ControlBox = false; // ×ボタンを非表示にして「閉じる」ボタンのみ使う

            // 説明文
            Label lbl = new Label();
            lbl.Text = message;
            lbl.Font = new Font("Meiryo UI", 14);
            lbl.Dock = DockStyle.Fill;
            lbl.Padding = new Padding(15);

            // 閉じるボタン
            Button close = new Button();
            close.Text = "閉じる";
            close.Dock = DockStyle.Bottom;
            close.Click += (s, e) => info.Close();

            // フォームに配置
            info.Controls.Add(lbl);
            info.Controls.Add(close);

            // 親フォームがあればモーダルで表示
            if (parent != null)
                info.ShowDialog(parent);
            else
                info.ShowDialog();
        }

        public static void MoziKesu(TextBox txt)
        {
            txt.Text = "";
            txt.ForeColor = Color.Black;
            txt.Focus();        //テキストボックスに選択を渡す
        }

    }
}
