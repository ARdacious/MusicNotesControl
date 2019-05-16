using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Drawing.Drawing2D;

namespace MusicNotes
{
    [Designer(typeof(MusicNotes.Design.MusicNotesControlDesign), typeof(IRootDesigner))]
    public partial class MusicNotesControll: UserControl
    {
        public MusicNotesControll()
        {
            InitializeComponent();
            
        }
        Brush back = new SolidBrush(Color.White);
        Pen notePen = new Pen(Color.Black, 2);
        Pen noteBarPen = new Pen(Color.Black, 3);
        Pen barlinePen = new Pen(Color.Black, 1);
        Brush noteBrush = new SolidBrush(Color.Black);
        Brush noteRed = new SolidBrush(Color.Red);
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // draw background
            g.FillRectangle(back, 0, 0, Width, Height);
            g.TranslateTransform(10, 45);
            // draw some lines
            DrawLines(g, 5);
            // draw some notes
            int cnt = 1;
            DrawNote(g, noteRed, 69, 1, 1);
            for (int midiNote = 69 -8; midiNote < 69 + 8; midiNote++)
            {
                cnt += 1;
                DrawNote(g, noteBrush, midiNote, cnt%8, cnt);
            }

        }
        int LineHeight = 5;
        int NoteSpacing = 30;
        protected void DrawNote(Graphics g, Brush brush, int note, int length, int time)
        {
            // base A4 midi note
            note = 69 - note + 4;
            int numBars = length - 3;
            int tailLenght = numBars > 2 ? 30 : 25;
            var s = g.Save();
            g.TranslateTransform(time * NoteSpacing, (note * LineHeight) + 2);
            g.RotateTransform(-15f);
            // draw note elipse (fill or open)
            switch(length)
            {
                case 1:
                case 2:
                    g.DrawEllipse(notePen, 1, 1, dim.X-2, dim.Y-2);
                    break;
                default:
                    g.FillEllipse(brush, 0, 0, dim.X, dim.Y);
                    break;
            }
            // draw line from elipse (for all but full length note)
            switch (length)
            {
                case 1:
                    break;
                default:
                    g.RotateTransform(+30f);
                    g.DrawLine(notePen, dim.X, 0, dim.X, -tailLenght);
                    break;
            }
            var pts = new List<Point>();
            for (int i = 0; i < numBars; i++)
                pts.Add(new Point(dim.X, -tailLenght + (i * 6)));
            var ptsa = pts.ToArray();
            //g.TransformPoints(CoordinateSpace.Page, CoordinateSpace.World, pts);
            if (pts.Any())
                g.Transform.TransformPoints(ptsa);
            g.Restore(s);
            var t = g.Transform.Clone();
            t.Invert();
            if (pts.Any())
                t.TransformPoints(ptsa);
            // draw sideway line
            foreach (Point p in ptsa )
            {
                g.DrawLine(noteBarPen, p, new Point(p.X + 20, p.Y));
            }

        }
        protected void DrawLines(Graphics g, int count)
        {
            for (int i = 0; i < count; i++)
            {
                g.DrawLine(barlinePen, 0, i * LineHeight * 2, Width, i * LineHeight * 2);
            }
        }
        private Point dim = new Point(15,10);
        [Browsable(true)]
        public Point NoteSize
        {
            get
            {
                return this.dim;
            }
            set
            {
                dim = value;
                LineHeight = dim.Y / 2;
            }
        }
        private string demoStringValue = null;
        [Browsable(true)]
        public string DemoString
        {
            get
            {
                return this.demoStringValue;
            }
            set
            {
                demoStringValue = value;
            }
        }
    }
}
