﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace StarboundAnimator
{
    public partial class Form1 : Form
    {
		public const int ASSET_IMAGELIST_FOLDER = 0;
		public const int ASSET_IMAGELIST_FOLDERBAD = 1;
		public const int ASSET_IMAGELIST_ANIMATION = 2;
		public const int ASSET_IMAGELIST_ANIMATIONBAD = 3;
		public const int ASSET_IMAGELIST_FRAMES = 4;
		public const int ASSET_IMAGELIST_FRAMESBAD = 5;
		public const int ASSET_IMAGELIST_FRAMEGRID = 6;
		public const int ASSET_IMAGELIST_FRAMELIST = 7;
		public const int ASSET_IMAGELIST_FRAMESUNTESTED = 8;
		public const int ASSET_IMAGELIST_ANIMATIONUNTESTED = 9;


		public FramesProperties FramesProperties = new FramesProperties();


        public Form1()
        {
            InitializeComponent();

			Globals.AppForm = this;
			Globals.AppPath = Application.StartupPath;
			Globals.AppSettings = Settings.LoadSettings();
			if (Globals.AppSettings.WindowWidth > 0) Width = Globals.AppSettings.WindowWidth;
			if (Globals.AppSettings.WindowHeight > 0) Height = Globals.AppSettings.WindowHeight;
			if (Globals.AppSettings.ExplorerWidth > 0) splitContainer1.SplitterDistance = Globals.AppSettings.ExplorerWidth;
			if (Globals.AppSettings.PropertiesWidth > 0) splitContainer2.SplitterDistance = Globals.AppSettings.PropertiesWidth;
			Globals.AppSettings.ValidateSettings();
			foreach (CachedAsset ca in Globals.AppSettings.CachedAssets)
			{
				AddAssetPath(ca.Title, ca.Assets, null);
			}
			// load last project here

			if (Globals.AppSettings.PathToLastProject == "") tabPages.SelectedTab = tabAssets;
        }



		public void RemoveAssetGroupByTitle(string title)
		{
			for (int i = 0; i < tvAssets.Nodes.Count; i++)
				if (tvAssets.Nodes[i].Text == title)
				{
					// need to go through all nodes to ensure animations and frames are unloaded properly
					tvAssets.Nodes.RemoveAt(i--);
					break;
				}
		}

		public void ChangeAssetTitle(string oldtitle, string newtitle)
		{
			for (int i = 0; i < tvAssets.Nodes.Count; i++)
			{
				if (tvAssets.Nodes[i].Text == oldtitle)
				{
					tvAssets.Nodes[i].Text = newtitle;
					break;
				}
			}
		}

		public List<string> AddAssetPath(string title, string path, TreeNode root)
		{
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
			{
				TreeNode tn = new TreeNode(title, ASSET_IMAGELIST_FOLDERBAD, ASSET_IMAGELIST_FOLDERBAD);
				tvAssets.Nodes.Add(tn);
				return new List<string>();
			}
			else
			{
				ScanPathForm spf = new ScanPathForm();
				spf.StartScan(path);

				if (spf.Found.Count > 0)
				{
					AddAssetPath(title, spf.Found, root);
					return spf.Found;
				}
				else
				{
					TreeNode tn = new TreeNode(title, ASSET_IMAGELIST_FOLDERBAD, ASSET_IMAGELIST_FOLDERBAD);
					tvAssets.Nodes.Add(tn);
					return new List<string>();
				}
			}
		}


		public void AddAssetPath(string title, List<string> foundlist, TreeNode root)
		{
			if ((foundlist == null) || (foundlist.Count == 0))
			{
				TreeNode tn = new TreeNode(title, ASSET_IMAGELIST_FOLDERBAD, ASSET_IMAGELIST_FOLDERBAD);
				if (root != null) root.Nodes.Add(tn);
				else tvAssets.Nodes.Add(tn);
			}
			else
			{
				LockAssetTreeView(true);

				foundlist.Sort();
				List<TreeNode> curnodepath = new List<TreeNode>();
				TreeNode tn = new TreeNode(title, ASSET_IMAGELIST_FOLDER, ASSET_IMAGELIST_FOLDER);
				if (root != null) root.Nodes.Add(tn);
				else tvAssets.Nodes.Add(tn);
				curnodepath.Add(tn);
				string[] split;
				char[] splitchar = { '\\' };
				for (int i = 0; i < foundlist.Count; i++)
				{
					split = foundlist[i].Split(splitchar);
					// because each foundlist entry starts with \, split[0] will always be "", so we skip it
					for (int j = 1; j < split.Length - 1; j++)
					{
						// node doesn't exist
						if (curnodepath.Count == j)
						{
							tn = new TreeNode(split[j], ASSET_IMAGELIST_FOLDER, ASSET_IMAGELIST_FOLDER);
							TreeNode pn = curnodepath[curnodepath.Count - 1];
							int n = 0;
							for (n = pn.Nodes.Count - 1; n >= 0; n--)
							{
								if (!(pn.Nodes[n] is AssetTreeNode)) break;
							}
							if (n == pn.Nodes.Count - 1) pn.Nodes.Add(tn);
							else pn.Nodes.Insert(n + 1, tn);
							curnodepath.Add(tn);
						}
						// node doesn't match
						else if (curnodepath[j].Text != split[j])
						{
							curnodepath.RemoveRange(j, curnodepath.Count - j);
							tn = new TreeNode(split[j], ASSET_IMAGELIST_FOLDER, ASSET_IMAGELIST_FOLDER);
							curnodepath[curnodepath.Count - 1].Nodes.Add(tn);
							curnodepath.Add(tn);
						}
					}

					// in case we dropped to a shorter path
					if (split.Length <= curnodepath.Count)
					{
						curnodepath.RemoveRange(split.Length - 1, curnodepath.Count - split.Length + 1);
					}

					// adjust this to set animation or frame image
					int tni = 0;
					if (split[split.Length - 1].EndsWith("animation")) tni = ASSET_IMAGELIST_ANIMATIONUNTESTED;
					else if (split[split.Length - 1].EndsWith("frames")) tni = ASSET_IMAGELIST_FRAMESUNTESTED;
					curnodepath[curnodepath.Count - 1].Nodes.Add(new AssetTreeNode(split[split.Length - 1], tni, tni, null));
				}

				LockAssetTreeView(false);
			}
		}

		public void LockAssetTreeView(bool bLock)
		{
			if (bLock) tvAssets.BeginUpdate();
			else tvAssets.EndUpdate();
		}

		void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SettingsForm sf = new SettingsForm();
			sf.ShowDialog();
		}

		void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			Globals.AppSettings.WindowWidth = Width;
			Globals.AppSettings.WindowHeight = Height;
			Globals.AppSettings.ExplorerWidth = splitContainer1.SplitterDistance;
			Globals.AppSettings.PropertiesWidth = splitContainer2.SplitterDistance;

			Globals.AppSettings.SaveSettings();
		}

		void SetWorkingFrames(Frames frame)
		{
			if (Globals.WorkingFrames != null) UnsetWorkingFrames();
			if (Globals.WorkingAnimation != null) UnsetWorkingAnimation();

			Globals.WorkingFrames = frame;
			tbSource.Text = frame.Source;
			FramesProperties.InitForFrames(frame);
			pgProperties.SelectedObject = FramesProperties;
			pnlEditor.UpdateFrameShowButtons(true);
		}

		void UnsetWorkingFrames()
		{
			if (Globals.WorkingFrames != null)
			{
				pgProperties.SelectedObject = null;
				FramesProperties.Frame = null;
				Globals.WorkingFrames = null;
				tbSource.Text = "";
			}

			pnlEditor.UpdateFrameShowButtons(false);
		}

		void SetWorkingAnimation(Animation anim)
		{
			if (Globals.WorkingFrames != null) UnsetWorkingFrames();
			if (Globals.WorkingAnimation != null) UnsetWorkingAnimation();

			Globals.WorkingAnimation = anim;
			tbSource.Text = anim.Source;
		}

		void UnsetWorkingAnimation()
		{
			if (Globals.WorkingAnimation != null)
			{
				Globals.WorkingAnimation = null;
				tbSource.Text = "";
			}
		}

		string GetPathForNode(TreeNode tn)
		{
			string path = "";
			string root = "";

			while (tn != null)
			{
				if (tn.Parent == null)
				{
					path = "\\" + path;
					root = tn.Text;
				}
				else
				{
					if (path == "") path = tn.Text;
					else path = tn.Text + '\\' + path;
				}

				tn = tn.Parent;
			}

			CachedAsset ca = Globals.AppSettings.CachedAssets.Find(tca => root == tca.Title);
			if (ca != null) return ca.Path + path;
			else return "";
		}

		private void tvAssets_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeNode tn = tvAssets.GetNodeAt(e.X, e.Y);
				tvAssets.SelectedNode = tn;
				cmsAssets.Show(tvAssets, e.X, e.Y);
			}
		}

		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (tvAssets.SelectedNode.Parent == null)
			{
				string title = tvAssets.SelectedNode.Text;
				string path = Globals.AppSettings.GetPathForTitle(tvAssets.SelectedNode.Text);

				// change this to a proper unload+clear
				TreeNode sn = tvAssets.SelectedNode;
				tvAssets.SelectedNode = null;
				tvAssets.Nodes.Remove(sn);
				CachedAsset ca = Globals.AppSettings.CachedAssets.Find(tca => title == tca.Title);
				ca.Assets = AddAssetPath(title, path, null);
			}
			else if (tvAssets.SelectedNode.Text.EndsWith(".animation"))
			{
			}
			else if (tvAssets.SelectedNode.Text.EndsWith(".frames"))
			{
			}
			else
			{
				string title = tvAssets.SelectedNode.Text;
				string path = "";
				string root = "";

				TreeNode tn = tvAssets.SelectedNode;
				while (tn != null)
				{
					if (tn.Parent == null)
					{
						path = "\\" + path;
						root = tn.Text;
					}
					else
					{
						if (path == "") path = tn.Text;
						else path = tn.Text + '\\' + path;
					}

					tn = tn.Parent;
				}

				// change this to a proper unload+clear
				TreeNode sn = tvAssets.SelectedNode;
				TreeNode pn = sn.Parent;
				tvAssets.SelectedNode = null;
				tvAssets.Nodes.Remove(sn);
				CachedAsset ca = Globals.AppSettings.CachedAssets.Find(tca => root == tca.Title);
				ca.Assets.RemoveAll(tca => tca.StartsWith(path + '\\'));
				List<string> foundlist = AddAssetPath(title, ca.Path + path, pn);
				int i = ca.Assets.IndexOf(ca.Path + path);
				ca.Assets.InsertRange(i + 1, foundlist);
			}
		}

		private void tvAssets_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Text.EndsWith(".frames"))
			{
				UnsetWorkingAnimation();

				Frames frame = null;
				if ((e.Node as AssetTreeNode).Asset != null)
				{
					frame = (e.Node as AssetTreeNode).Asset as Frames;
					pgProperties.SelectedObject = frame;
				}
				else
				{
					string framepath = GetPathForNode(e.Node);
					frame = Frames.LoadFromFile(framepath);
					if (frame != null)
					{
						(e.Node as AssetTreeNode).Asset = frame;
						e.Node.ImageIndex = ASSET_IMAGELIST_FRAMES;
						e.Node.SelectedImageIndex = ASSET_IMAGELIST_FRAMES;

						if (string.IsNullOrEmpty(frame.image))
						{
							string imagepath = framepath.Substring(0, framepath.Length - Path.GetFileName(framepath).Length);
							string imagename = Path.GetFileNameWithoutExtension(framepath) + ".png";
							if (File.Exists(imagepath + imagename))
							{
								DialogResult dr = MessageBox.Show("Image is unset! Do you want to try using " + imagename + "?", "Missing image", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
								if (dr == DialogResult.Yes)
								{
									frame.image = imagename;
									frame.Img = Image.FromFile(imagepath + imagename);
								}
							}
						}
						else
						{
							// load the referenced image
							// going to have issues with images using relative paths

							string imagepath = "";

							if (frame.image.Contains("/"))
							{
							}
							else
							{
								imagepath = framepath.Substring(0, framepath.Length - Path.GetFileName(framepath).Length) + frame.image;
							}

							frame.Img = Image.FromFile(imagepath);
						}

						if (frame.Img == null)
						{
							int w = 0;
							int h = 0;
							if (frame.frameGrid != null)
							{
								w = frame.frameGrid.size[0] * frame.frameGrid.dimensions[0];
								h = frame.frameGrid.size[1] * frame.frameGrid.dimensions[1];
							}
							else if (frame.frameList != null)
							{
							}
							if ((w > 0) && (h > 0))
							{
								Bitmap bmp = new Bitmap(w, h);
								using (Graphics graph = Graphics.FromImage(bmp))
								{
									Rectangle ImageSize = new Rectangle(0, 0, w, h);
									graph.FillRectangle(Brushes.Black, ImageSize);
								}
								frame.Img = bmp;
							}
						}

						pgProperties.SelectedObject = frame;
					}
					else
					{
						e.Node.ImageIndex = ASSET_IMAGELIST_FRAMESBAD;
						e.Node.SelectedImageIndex = ASSET_IMAGELIST_FRAMESBAD;
					}
				}

				if (frame != null) SetWorkingFrames(frame);
				else tbSource.Text = "";

				if (tabWorkspace.SelectedTab == tabEditor) tabEditor.Invalidate();
				else tabWorkspace.SelectedTab = tabEditor;
				tabWorkspace.Visible = true;
			}
			else if (e.Node.Text.EndsWith(".animation"))
			{
				Animation anim = null;
				if ((e.Node as AssetTreeNode).Asset != null)
				{
					anim = (e.Node as AssetTreeNode).Asset as Animation;
				}
				else
				{
					anim = Animation.LoadFromFile(GetPathForNode(e.Node));
					//anim = new Animation(GetPathForNode(e.Node));
					(e.Node as AssetTreeNode).Asset = anim;
				}

				if (anim != null) SetWorkingAnimation(anim);
				else tbSource.Text = "";

				tabWorkspace.SelectedTab = tabSource;
				tabWorkspace.Visible = true;
			}
			else if (e.Node != null)
			{
				// clear out whatever was there

				tabWorkspace.Visible = false;
				tbSource.Text = "";
			}
		}

		private void unpackStarboundAssetsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			/* check registry for Steam install location
			if exists, check for File.Exists("Steam/Steamapps/common/Starbound/Win32/asset_unpacker.exe")
			{
			}
			else
			{
				prompt for location of starbound directory
				if !File.Exists(asset_unpacker.exe) MessageBox.Show("you're an idiot");
			}
			open up find directory dialog to select where to unpack assets to
			have to ensure that selected directory is empty
			run asset_unpacker with appropriate selected settings
			*/
		}

		private void pgProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			pnlEditor.Invalidate();
		}
    }
}
