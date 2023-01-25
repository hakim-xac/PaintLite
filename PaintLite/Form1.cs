using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PaintLite
{
    public partial class Form1 : Form
    {
        private bool checkIsImageInit()
        {
            if (pictureBox1.Image != null)
            {
                switch (MessageBox.Show("Сохранить текущее изображение?", "Внимание!", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes: saveFile(); break;
                    case DialogResult.Cancel: return false;
                }
            }
            return true;
        }

        private bool checkIsImageNull()
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Сначала создайте новый файл!");
                return true;
            }
            return false;
        }

        private void colorSelect()
        {
            ColorDialog cd = new();
            if (cd.ShowDialog() == DialogResult.OK) history_color_ = cd.Color;
            cd.Dispose();
        }
        private void historyInit()
        {

            if (pictureBox1.Image == null) return;

            if(history_image_.Count > 0) history_image_.Clear();
            history_image_.Add(new Bitmap(pictureBox1.Image));
        }

        private void exit()
        {
            Application.Exit();
        }
        private void openFile()
        {
            if (!checkIsImageInit()) return;
            OpenFileDialog ofd = new();
            ofd.Filter = "JPEG Images (.jpeg)|*.jpg;*.jpeg|" +
                "PNG Images (.png)|*.png|" +
                "GIF Images (.gif)|*.gif|" +
                "Bitmap Images (.bmp)|*.bmp|" +
                "All Files(*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image?.Dispose();
                if (ofd.FileName == String.Empty) return;
                pictureBox1.Load(ofd.FileName);
                pictureBox1.AutoSize = true;
                pictureBox1.BackColor = eraser_color_;
            }

            historyInit();
        }
        private void saveFile()
        {
            if (pictureBox1.Image == null)
            {
                MessageBox.Show("Нечего сохранять!");
                return;
            }
            SaveFileDialog sfd = new(); 
            sfd.Filter = "JPEG Images (.jpeg)|*.jpg;*.jpeg|" +
                "PNG Images (.png)|*.png|" +
                "GIF Images (.gif)|*.gif|" +
                "Bitmap Images (.bmp)|*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FileName == String.Empty) return;

                FileStream fs = (FileStream)sfd.OpenFile();
                switch (sfd.FilterIndex)
                {
                    case 1: pictureBox1.Image.Save(fs, ImageFormat.Jpeg); break;
                    case 2: pictureBox1.Image.Save(fs, ImageFormat.Png); break;
                    case 3: pictureBox1.Image.Save(fs, ImageFormat.Gif); break;
                    case 4: pictureBox1.Image.Save(fs, ImageFormat.Bmp); break;
                }
                fs.Close();
            }
            sfd.Dispose();
        }

        private void newFile()
        {
            if (!checkIsImageInit()) return;
            Bitmap pic = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pic.MakeTransparent(eraser_color_);
            pictureBox1.BackColor = eraser_color_;
            pictureBox1.Image?.Dispose();
            pictureBox1.Image= pic;

            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            g.DrawImage(pictureBox1.Image, 0, 0, pictureBox1.Width, pictureBox1.Height);

            historyInit();
        }



        public Form1()
        {
            InitializeComponent();
            drawing_ = false;
            history_color_ = Color.Black;
            eraser_color_ = Color.White;
            current_pen_ = new Pen(history_color_);

            current_pen_.Width = trackBar1.Value;
            label1.Text = "0, 0";
            history_image_ = new List<Image>();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            newFile();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            openFile();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Разработчик:\tХакимов А.С.\nГруппа:\t\tПБ-11");
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFile();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if(checkIsImageNull()) return;
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;

            drawing_ = true;
            old_location_ = e.Location;
            current_path_ = new GraphicsPath();
            label1.Text = e.X.ToString() + ", " + e.Y.ToString();
            switch (e.Button)
            {
                case MouseButtons.Left:     current_pen_.Color = history_color_;  break;
                case MouseButtons.Right:    current_pen_.Color = eraser_color_; break;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (checkIsImageNull()) return;
            history_image_.Add(new Bitmap(pictureBox1.Image));
            ++history_current_index_;
            if (history_image_.Count > history_max_count_)
            {
                history_image_.RemoveAt(0);
                history_current_index_ = history_max_count_ - 1;
            }
            if (history_image_.Count - 1 > history_current_index_)
            {
                var index = history_current_index_;
                var count = history_image_.Count - index - 1;

                history_image_.RemoveRange(index, count);

                Console.WriteLine("index:\t"+ index.ToString());
                Console.WriteLine("count:\t" + count.ToString());
            }
            drawing_= false;
            current_path_?.Dispose();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!drawing_ || pictureBox1.Image == null) return;

            Graphics gr = Graphics.FromImage(pictureBox1.Image);
            current_path_.AddLine(old_location_, e.Location);
            gr.DrawPath(current_pen_, current_path_);
            old_location_ = e.Location;
            gr.Dispose();
            pictureBox1.Invalidate();
            label1.Text = e.X.ToString() + ", " + e.Y.ToString();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            if (checkIsImageNull()) return;

            pictureBox1.Width = this.Width;
            pictureBox1.Height = this.Height;
            pictureBox1.Update();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            current_pen_.Width = trackBar1.Value;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (history_image_.Count <= 1 || history_current_index_ == 0)
            {
                MessageBox.Show("История пуста!");
                return;
            }
            pictureBox1.Image = new Bitmap(history_image_[--history_current_index_]);
        }

        private void renoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(history_current_index_ + 1 >= history_image_.Count && history_image_.Count == history_max_count_)
            {
                MessageBox.Show("История пуста!");
                return;
            }
            pictureBox1.Image = new Bitmap(history_image_[++history_current_index_]);
        }

        private void solidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            current_pen_.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = true;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;
        }

        private void dotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            current_pen_.DashStyle = DashStyle.Dot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = true;
            dashDotDotToolStripMenuItem.Checked = false;
        }

        private void dashDotDotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            current_pen_.DashStyle = DashStyle.DashDotDot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = true;
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorSelect();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            colorSelect();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!checkIsImageInit()) return;
        }
    }
}