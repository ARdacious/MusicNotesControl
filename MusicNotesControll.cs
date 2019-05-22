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

        List<Note> notes = new List<Note>();

        string unicodestuff = "\u266d \u266f \u266e"; // bes sharp normal

        public MusicNotesControll()
        {
            InitializeComponent();
            notes.Add(new Note('A', 4, 0, false));
            notes.Add(new Note('B', 4, 1, true));
            notes.Add(new Note('E', 4, -1, true));
            notes.Add(new Note('A', 4, 0, true));
            notes.Add(new Note('A', 4, 0, false));
            notes.Add(new Note('G', 4, 0, false));
            notes.Add(new Note('A', 5, 0, false));
            notes.Add(new Note('D', 3, 0, false));
            notes.Add(new Note('E', 3, 0, false));
            notes.Add(new Note('F', 5, 0, false));
            notes.Add(new Note('A', 4, 0, false));

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
            // draw content
            drawDefaultNotes(g);
            
        }
        public void drawDefaultNotes(Graphics g)
        {
            // draw some notes
            int cnt = 0;
            foreach (Note note in notes)
            {
                cnt += 1;
                //Console.WriteLine("{0}: {1} {2}", on, l, on.NoteLength);
                DrawNote(g, noteBrush, note.Step, note.Octave, note.Alter, note.ShowAlter, cnt%8, cnt*6);
            }

        }
        int LineHeight = 5;
        int NoteSpacing = 8;
        string[] alterLookUp = new string[] { "\u266d", "\u266e", "\u266f" };
        protected void DrawNote(Graphics g, Brush brush, char note, int octave, int alter, bool showalter, int length, int time)
        {
            // base A4 midi note
            var noteline = 'A' - note + 4 ;
            noteline += (octave - 4) * -8; // do not count the black keys
            Console.WriteLine("linenr {0}", noteline);
            // first three notes do not have a top bar
            int numBars = length - 3;
            // longer note tail for more bars
            int tailLenght = numBars > 2 ? 30 : 25;
            var s = g.Save();
            // translate to note offset in X
            g.TranslateTransform(time * NoteSpacing, 0);
            if (noteline < -2)
            {
                // odd lines need to draw when not on the available 5 lines drawn
                for (int i = -3; i >= noteline; i-=2)
                    g.DrawLine(barlinePen, -5, (i * LineHeight), dim.X+5, i*LineHeight);
            }
            if (noteline > 8)
            {

            }
            // translate to note position in Y
            g.TranslateTransform(0, (noteline * LineHeight) + 2);
            // draw alter (5 and up is special for not drawing)
            if (showalter)
            {
                var m = g.MeasureString(alterLookUp[1 + alter], drawFont);
                //Console.WriteLine("measured {0} for {1}", m, alter);
                g.DrawString(alterLookUp[1 + alter], drawFont, drawBrush, -(Math.Min(22, m.Width) - 2), -fontSize / 2, drawFormat);
            }
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
                g.DrawLine(noteBarPen, p, new Point(p.X + 10, p.Y));
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
    public class Note
    {
        //                                    A,  B, C  D  E  F  G 
        static int[] MidiLookup = new int[] { 9, 11, 0, 2, 4, 5, 7 }; // from C = 0
        public Note(char step, int octave, int alter, bool show)
        {
            Step = step;
            Octave = octave;
            Alter = alter;
            ShowAlter = show;
            Midi = (octave * 12) + MidiLookup[step - 'A'] + alter;
        }
        public char Step;
        public int Octave;
        public int Alter;
        public bool ShowAlter;
        public int Midi;
    }
}
