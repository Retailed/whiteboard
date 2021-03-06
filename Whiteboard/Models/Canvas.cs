﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Whiteboard.Models
{
    public class Canvas
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        // Primary key
        public Guid Id { get; set; }

        // Id of room
        public Guid RoomId { get; set; }
        // Content of canvas
        private byte[] Content;
        // Last time of modification
        public DateTime ModifiedAt { get; set; }
        // Buffer image object
        [NotMapped]
        private Image buffer;
        [NotMapped]
        private readonly object canvasLock = new object();

        // Pixel size
        private static int pixelSize = 4; 

        public byte[] GetBytes()
        {
            Flush();
            byte[] tmp = new byte[Content.Length];
            lock (canvasLock)
                Content.CopyTo(tmp, 0);
            return tmp;
        }

        public Image GetImage()
        {
            lock (canvasLock)
            {
                if (buffer == null)
                    InitBuffer();
                return new Bitmap(buffer);
            }
        }

        private void InitBuffer()
        {
            if (Content == null)
            {
                buffer = new Bitmap(1000, 600);
                Content = new byte[buffer.Width * buffer.Height * pixelSize];
            }
            else
            {
                buffer = new Bitmap(new MemoryStream(Content));
            }
        }

        public void DrawLine(Color color, Point from, Point to)
        {
            lock (canvasLock)
            {
                if (buffer == null)
                    InitBuffer();
                using (var g = Graphics.FromImage(buffer))
                {
                    g.DrawLine(new Pen(color), from, to);
                    g.Flush();
                }
            }
        }

        public void Flush()
        {
            lock (canvasLock)
            {
                if (buffer == null)
                    InitBuffer();
                buffer.Save(new MemoryStream(Content), ImageFormat.Png);
            }
        }
    }
}
