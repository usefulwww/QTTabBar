using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace QuizoPlugins
{
	public partial class MigemoOptionForm : Form
	{
		public string pathDLL, pathDic;
		public bool fPartialMatch;

		public MigemoOptionForm( string pathDLL, string pathDic, bool fPartialMatch )
		{
			this.pathDLL = pathDLL;
			this.pathDic = pathDic;
			this.fPartialMatch = fPartialMatch;

			InitializeComponent();

			this.textBoxDLL.Text = this.pathDLL;
			this.textBoxDic.Text = this.pathDic;
			this.checkBox1.Checked = this.fPartialMatch;
		}

		private void button_Browse_Click( object sender, EventArgs e )
		{
			using( OpenFileDialog ofd = new OpenFileDialog() )
			{
				bool fDLL = sender == this.buttonBrowseDll;

				if( fDLL )
				{
					ofd.Filter = "Migemo dll file (*.dll)|*.dll";
					ofd.FileName = this.pathDLL;
				}
				else
				{
					ofd.Filter = "Dictionary file ( migemo-dict )|*.*";
					ofd.FileName = this.pathDic;
				}

				if( DialogResult.OK == ofd.ShowDialog() )
				{
					if( fDLL )
					{
						this.textBoxDLL.Text = ofd.FileName;
					}
					else
					{
						this.textBoxDic.Text = ofd.FileName;
					}
				}
			}
		}

		private void buttonOK_Click( object sender, EventArgs e )
		{
			this.DialogResult = DialogResult.OK;
		}

		private void checkBox1_CheckedChanged( object sender, EventArgs e )
		{
			this.fPartialMatch = this.checkBox1.Checked;
		}

		private void textBoxes_TextChanged( object sender, EventArgs e )
		{
			if( sender == this.textBoxDLL )
			{
				this.pathDLL = this.textBoxDLL.Text;
				this.textBoxDLL.ForeColor = File.Exists( this.pathDLL ) ? this.ForeColor : Color.Red;
			}
			else
			{
				this.pathDic = this.textBoxDic.Text;
				this.textBoxDic.ForeColor = File.Exists( this.pathDic ) ? this.ForeColor : Color.Red;
			}
		}
	}
}