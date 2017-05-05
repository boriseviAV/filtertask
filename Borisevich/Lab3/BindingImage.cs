using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace ImageInfoViewer
{
    class BindingImage
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public float HorisontalResolution { get; set; }
        public float VerticalResolution { get; set; }
        public string CompressionType { get; set; }
        public int ColorDepth { get; set; }

        private string GetCompressionType(Image image)
        {
            int compressionTagIndex = Array.IndexOf(image.PropertyIdList, 0x103);
            int Type = 0;
            if (compressionTagIndex > -1)
            {
                PropertyItem compressionTag = image.PropertyItems[compressionTagIndex];
                Type = BitConverter.ToInt16(compressionTag.Value, 0);
            }
            string Result = "No compression";
            switch (Type)
            {
                case 2:
                    Result = "CCITT modified Huffman RLE";
                    break;
                case 3:
                    Result = "CCITT Group 3 fax encoding";
                    break;
                case 4:
                    Result = "CCITT Group 4 fax encoding";
                    break;
                case 5:
                    Result = "LZW";
                    break;
                case 6:
                    Result = "'old-style' JPEG";
                    break;
                case 7:
                    Result = "'new-style' JPEG";
                    break;
                case 32773:
                    Result = "Macintosh RLE";
                    break;
            }
            return Result;
        }

        private string GetPcxCompressionType(byte flag)
        {
            if (flag == 0)
                return "No compression";
            else
                return "RLE";
        }

        public BindingImage(int num, string name, Image image)
        {
            ColorDepth = Image.GetPixelFormatSize(image.PixelFormat);
            Number = num;
            Name = name;
            Format = new ImageFormatConverter().ConvertToString(image.RawFormat);
            Height = image.Height;
            Width = image.Width;
            HorisontalResolution = image.HorizontalResolution;
            VerticalResolution = image.VerticalResolution;
            CompressionType = GetCompressionType(image);
        }

        public BindingImage(int num, string name, FileStream PcxImage)
        {
            Number = num;
            Name = name;
            Format = "Pcx";
            byte[] WordArray = new byte[2];
            byte[] ByteArray = new byte[1];

            short XStart, XEnd;
            short YStart, YEnd;

            PcxImage.Position = 12;
            PcxImage.Read(WordArray, 0, 2);
            HorisontalResolution = (float)BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 14;
            PcxImage.Read(WordArray, 0, 2);
            VerticalResolution = (float)BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 3;
            PcxImage.Read(WordArray, 0, 1);
            ColorDepth = BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 2;
            PcxImage.Read(ByteArray, 0, 1);
            CompressionType = GetPcxCompressionType(ByteArray[0]);

            PcxImage.Position = 4;
            PcxImage.Read(WordArray, 0, 2);
            XStart = BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 6;
            PcxImage.Read(WordArray, 0, 2);
            YStart = BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 8;
            PcxImage.Read(WordArray, 0, 2);
            XEnd = BitConverter.ToInt16(WordArray, 0);

            PcxImage.Position = 10;
            PcxImage.Read(WordArray, 0, 2);
            YEnd = BitConverter.ToInt16(WordArray, 0);

            Width = XEnd - XStart;
            Height = YEnd - YStart;
        }
    }
}
