using SystemImage = System.Drawing.Image;
using SystemBitmap = System.Drawing.Bitmap;
using SystemPoint = System.Drawing.Point;
using SystemGraphics = System.Drawing.Graphics;
using SystemRectangle = System.Drawing.Rectangle;
using SystemSize = System.Drawing.Size;
using SystemFont = System.Drawing.Font;
using SystemFontStyle = System.Drawing.FontStyle;

public class ImageHandler
{
    public SystemImage CropImage(SystemImage img, SystemRectangle cropArea)
    {
        using (var bpmImage = new SystemBitmap(img))
        {
            return bpmImage.Clone(cropArea, bpmImage.PixelFormat);
        }
    }

    public SystemImage ScaleImage(SystemBitmap bmp, int maxWidth, int maxHeight)
    {

        double ratio;

        double ratioX;
        double ratioY;


        if (bmp.Height < bmp.Width)
        {
            ratio = (double)maxHeight / bmp.Height;
        }
        else if (bmp.Height > bmp.Width)
        {
            ratio = (double)maxWidth / bmp.Width;
        }
        else
        {
            ratioX = (double)maxWidth / bmp.Width;
            ratioY = (double)maxHeight / bmp.Height;
            ratio = Math.Min(ratioX, ratioY);
        }

        var newWidth = (int)(bmp.Width * ratio);
        var newHeight = (int)(bmp.Height * ratio);

        var newImage = new SystemBitmap(newWidth, newHeight);

        using (var graphics = System.Drawing.Graphics.FromImage(newImage))
            graphics.DrawImage(bmp, 0, 0, newWidth, newHeight);

        return (SystemImage)newImage;
    }

    public void MakeSocialPost(string imagePath, string watermarkPath, string fontPath = null, string font = "iransans", string text = "بدون متن", string destinationPath = null)
    {
        if (destinationPath == null || imagePath.Contains(".jpg"))
        {
            destinationPath = imagePath.Replace(".jpg", "watermarked.jpg");
        }
        else if (destinationPath == null || imagePath.Contains(".png"))
        {
            destinationPath = imagePath.Replace(".png", "watermarked.png");
        }
        else
        {
            destinationPath = destinationPath ?? imagePath;
        }

        fontPath = fontPath ?? "Fonts";


        if (!System.IO.File.Exists(imagePath))
        {
            throw new BusinessException("Image file to be watermarked does not exist");
        }
        if (!System.IO.File.Exists(watermarkPath))
        {
            throw new BusinessException("Watermark file does not exist");
        }

        char[] persianChars = { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹', '،', '؛' };
        char[] englishChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', ';' };
        string finalText = text;

        for (int i = 0; i < englishChars.Length; i++)
        {
            if (text.Contains(englishChars[i]))
            {
                string result = "";

                for (var j = 0; j < englishChars.Length; j++)
                {
                    result = text.Replace(englishChars[j], persianChars[j]);
                }

                finalText = result;
                break;
            }
        }

        SystemImage postImage = SystemImage.FromFile(imagePath);
        SystemBitmap postImageBitmap = new SystemBitmap(postImage);

        SystemImage scaledImage = ScaleImage(postImageBitmap, 1080, 1080);

        SystemRectangle cropArea;

        if (scaledImage.Width > scaledImage.Height)
        {
            int width = scaledImage.Width / 2 - 1080 / 2;
            cropArea = new SystemRectangle(new SystemPoint(width, 0), new SystemSize(1080, 1080));
        }
        else if (scaledImage.Width < scaledImage.Height)
        {
            int height = scaledImage.Height / 2 - 1080 / 2;
            cropArea = new SystemRectangle(new SystemPoint(0, height), new SystemSize(1080, 1080));
        }
        else
        {
            cropArea = new SystemRectangle(new SystemPoint(0, 0), new SystemSize(1080, 1080));
        }

        SystemImage croppedImage = CropImage(scaledImage, cropArea);

        float fontSize = 30f;

        SystemImage backgroundImage = croppedImage;
        SystemImage watermark = SystemImage.FromFile(watermarkPath);
        SystemGraphics watermarkGraphics = SystemGraphics.FromImage(backgroundImage);
        watermarkGraphics.DrawImage(watermark, new SystemPoint(0, backgroundImage.Height - watermark.Height));

        SystemImage target = backgroundImage;

        System.Drawing.Text.PrivateFontCollection pfc = new System.Drawing.Text.PrivateFontCollection();
        pfc.AddFontFile($"{fontPath}/Iransans/Iransans-Bold.otf");
        pfc.AddFontFile($"{fontPath}/Vazir/Vazir-Bold.otf");
        pfc.AddFontFile($"{fontPath}/Yekanbakh/YekanBakh-Bold.otf");
        System.Drawing.Color color = System.Drawing.Color.FromArgb(255, System.Drawing.Color.Black);
        System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(color);

        System.Drawing.StringFormat stringFormat = new System.Drawing.StringFormat();
        stringFormat.Alignment = System.Drawing.StringAlignment.Center;
        stringFormat.LineAlignment = System.Drawing.StringAlignment.Center;
        stringFormat.FormatFlags = System.Drawing.StringFormatFlags.DirectionRightToLeft;

        int fontFamilyIndex;

        switch (font)
        {
            case "iransans":
                fontFamilyIndex = 2;
                break;
            case "vazir":
                fontFamilyIndex = 1;
                break;
            case "yekan":
                fontFamilyIndex = 0;
                break;
            default:
                fontFamilyIndex = 2;
                break;
        }

        if (finalText.Length < 30)
        {
            fontSize = 65f;

            SystemPoint textStartingPoint = new SystemPoint(target.Width / 2, 810);

            SystemGraphics graphics = SystemGraphics.FromImage(target);
            graphics.DrawString(finalText, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, textStartingPoint, stringFormat);
            graphics.Save();
            target.Save(destinationPath);
            graphics.Dispose();
            pfc.Dispose();

        }

        if (text.Length >= 30 && text.Length < 40)
        {

            fontSize = 40f;

            SystemPoint textStartingPoint = new SystemPoint(target.Width / 2, 810);

            SystemGraphics graphics = SystemGraphics.FromImage(target);
            graphics.DrawString(finalText, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, textStartingPoint, stringFormat);
            graphics.Save();
            target.Save(destinationPath);
            graphics.Dispose();
            pfc.Dispose();
        }

        if (finalText.Length >= 40 && finalText.Length < 80)
        {

            int firstLineEnd = finalText.Substring(30).IndexOf(" ");
            int splitIndex = 30 + firstLineEnd + 1;
            string firstLine = finalText.Substring(0, splitIndex);
            string secondLine = finalText.Substring(splitIndex);

            fontSize = 40f;

            SystemPoint firstLinePoint = new SystemPoint(target.Width / 2, 810);
            SystemPoint secondLinePoint = new SystemPoint(target.Width / 2, 880);

            SystemGraphics graphics = SystemGraphics.FromImage(target);
            graphics.DrawString(firstLine, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, firstLinePoint, stringFormat);
            graphics.DrawString(secondLine, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, secondLinePoint, stringFormat);
            graphics.Save();
            target.Save(destinationPath);
            graphics.Dispose();
            pfc.Dispose();

        }

        if (finalText.Length >= 80)
        {

            int firstLineEnd = finalText.Substring(30).IndexOf(" ");
            int secondLineEnd = finalText.Substring(65).IndexOf(" ");
            int firstLineSplitIndex = 30 + firstLineEnd + 1;
            int secondLineSplitIndex = 65 + secondLineEnd + 2;
            string firstLine = finalText.Substring(0, firstLineSplitIndex);
            string secondLine = finalText.Substring(firstLineSplitIndex - 1, secondLineSplitIndex - firstLineSplitIndex - 1);
            string thirdLine;

            if (finalText.Substring(secondLineSplitIndex).Length > 40)
            {
                thirdLine = finalText.Substring(secondLineSplitIndex - 2, 37) + "...";
            }
            else
            {
                thirdLine = finalText.Substring(secondLineSplitIndex - 2);
            }

            fontSize = 35f;

            SystemPoint firstLinePoint = new SystemPoint(target.Width / 2, 780);
            SystemPoint secondLinePoint = new SystemPoint(target.Width / 2, 840);
            SystemPoint thirdLinePoint = new SystemPoint(target.Width / 2, 900);

            SystemGraphics graphics = SystemGraphics.FromImage(target);
            graphics.DrawString(firstLine, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, firstLinePoint, stringFormat);
            graphics.DrawString(secondLine, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, secondLinePoint, stringFormat);
            graphics.DrawString(thirdLine, new SystemFont(pfc.Families[fontFamilyIndex], fontSize, SystemFontStyle.Bold), brush, thirdLinePoint, stringFormat);
            graphics.Save();
            target.Save(destinationPath);
            graphics.Dispose();
            pfc.Dispose();
        }
    }
}
