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
using NAudio.Midi;

namespace MusicNotes
{
    [Designer(typeof(MusicNotes.Design.MusicNotesControlDesign), typeof(IRootDesigner))]
    public partial class MusicNotesControll: UserControl
    {
        public delegate void DrawMusicNotes(Graphics g);

        public List<Note> Notes = new List<Note>();

        public MusicNotesControll()
        {
            InitializeComponent();
            maxWidth = Width;
            Notes.Add(new Note('A', 4, 0, false, "whole", 16));
            Notes.Add(new Note('G', 3, 1, true, "quarter", 8));
            Notes.Add(new Note('A', 3, -1, true, "eighth", 4));
            Notes.Add(new Note('B', 3, 0, true, "16th", 2));
            Notes.Add(new Note('C', 4, 0, false, "32th", 1));
            Notes.Add(new Note('D', 4, 0, false, "whole", 16));
            Notes.Add(new Note('E', 4, 0, false, "whole", 16));
            Notes.Add(new Note('F', 4, 0, false, "whole", 16));
            Notes.Add(new Note('G', 4, 0, false, "whole", 16));
            Notes.Add(new Note('A', 4, 0, false, "whole", 16));
            Notes.Add(new Note('B', 4, 0, false, "whole", 16));
            Notes.Add(new Note('C', 5, 0, false, "whole", 16));

        }
        static int fontSize = 16;
        System.Drawing.Font drawFont = new System.Drawing.Font("Arial", fontSize, FontStyle.Bold);
        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
        System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
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
            // handle scroll
            g.TranslateTransform(-(scrollValue * maxWidth / 100), 0);
            // draw content
            drawDefaultNotes(g);
            
        }
        public void drawDefaultNotes(Graphics g)
        {
            // draw some notes
            int cnt = 0;
            foreach (Note note in Notes)
            {
                cnt += 1;
                //Console.WriteLine("{0}: {1} {2}", on, l, on.NoteLength);
                DrawNote(g, noteBrush, note.Step, note.Octave, note.Alter, note.ShowAlter, note.Type, cnt*6);
            }

        }
        int LineHeight = 5;
        int NoteSpacing = 8;
        
        protected void DrawNote(Graphics g, Brush brush, char step, int octave, int alter, bool showalter, string type, int time)
        {
            // base A4 midi note
            var noteline = (step - 'A' + 5) % 7;
            noteline += (octave - 4) * 7; // do not count the black keys
            noteline *= -1; // invert
            noteline += 9;
            //Console.WriteLine("linenr {0}", noteline);
            // first three notes do not have a top bar
            int numBars = 0;
            // longer note tail for more bars
            int tailLenght = numBars > 2 ? 30 : 25;
            var s = g.Save();
            // translate to note offset in X
            g.TranslateTransform(time * NoteSpacing, 0);
            if (noteline < -2)
            {
                // even lines need to draw when not on the available 5 lines drawn
                for (int i = -2; i > noteline; i-=2)
                    g.DrawLine(barlinePen, -5, i*LineHeight, dim.X+5, i*LineHeight);
            }
            if (noteline > 8)
            {
                // even lines need to draw when not on the available 5 lines drawn
                for (int i = 8; i <= noteline+1; i += 2)
                    g.DrawLine(barlinePen, -5, i*LineHeight, dim.X + 5, i*LineHeight);
            }
            // translate to note position in Y
            g.TranslateTransform(0, (noteline * LineHeight) + 2);
            // draw alter (5 and up is special for not drawing)
            if (showalter)
            {
                var m = g.MeasureString(Note.alterLookUp[1 + alter], drawFont);
                //Console.WriteLine("measured {0} for {1}", m, alter);
                g.DrawString(Note.alterLookUp[1 + alter], drawFont, drawBrush, -(Math.Min(22, m.Width) - 2), -fontSize / 2, drawFormat);
            }
            g.RotateTransform(-15f);
            // draw note elipse (fill or open)
            switch(type)
            {
                case "whole":
                case "half":
                    g.DrawEllipse(notePen, 1, 1, dim.X-2, dim.Y-2);
                    break;
                default:
                    g.FillEllipse(brush, 0, 0, dim.X, dim.Y);
                    break;
            }
            // draw line from elipse (for all but full length note)
            switch (type)
            {
                case "whole":
                    break;
                default:
                    g.RotateTransform(+30f);
                    g.DrawLine(notePen, dim.X, 0, dim.X, -tailLenght);
                    break;
            }
            switch (type)
            {
                case "eighth":
                    numBars = 1;
                    break;
                case "16th":
                    numBars = 2;
                    break;
                case "32th":
                    numBars = 3;
                    break;
                default:
                    numBars = 0;
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
                g.DrawLine(noteBarPen, p, new Point(p.X + 10, p.Y));
            }
            // determine max widht
            maxWidth = Math.Max(maxWidth, time * NoteSpacing);
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
        private int scrollValue = 0;
        private int maxWidth = 0;

        public int ScrollValue
        {
            get
            {
                return scrollValue;
            }
            set
            {
                scrollValue = value;
                Invalidate();
            }
        }


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

        private void MusicNotesControll_MouseEnter(object sender, EventArgs e)
        {
            Invalidate();
        }
    }

    public class Note
    {
        public static string[] alterLookUp = new string[] { "\u266d", "\u266e", "\u266f" };
        public static string[] alterLookUpASCII = new string[] { "b", "o", "#" };
        //                                    C  D  E  F  G  A  B
        static int[] MidiLookup = new int[] { 0, 2, 4, 5, 7, 9, 11 }; // from C = 0
        public Note(char step, int octave, int alter, bool show, string type, int duration)
        {
            Step = step;
            Octave = octave;
            Alter = alter;
            ShowAlter = show;
            var idx = (step - 'A' + 5) % 7;
            Midi = (octave * 12) + MidiLookup[idx] + alter + 12;
            Type = type;
            Duration = duration;
        }
        public override string ToString() {
            if (ShowAlter)
            {
                return string.Format("Note: {2}{0}{1} ({3})", Step, Octave, alterLookUpASCII[Alter + 1], Midi);
            }
            else
            {
                return string.Format("Note: {0}{1} ({3})", Step, Octave, alterLookUpASCII[Alter + 1], Midi);
            }
        }
        public char Step;
        public int Octave;
        public int Alter;
        public bool ShowAlter;
        public string Type;
        public int Midi;
        public int Duration;
    }
}
