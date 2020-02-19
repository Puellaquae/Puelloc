namespace Puelloc
{
    public class MIME
    {
        private readonly string _name;
        public override string ToString()
        {
            return _name;
        }
        public MIME(string name)
        {
            _name = name;
        }
    }
    public static class MIMEs
    {
		public static MIME TryParse(string extension)
        {
            extension = extension.ToLower().TrimStart('.');
            return extension switch
            {
                "txt" => TextPlainUTF8,
                "html" => TextHtml,
                "css" => TextCss,
                "js" => ApplicationJavascript,
                "json" => ApplicationJson,
                "jpg" => ImageJpeg,
                "jpeg" => ImageJpeg,
                "png" => ImagePng,
                "gif" => ImageGif,
                "svg" => ImageSvgXml,
                "wav" => AudioWav,
                "wave" => AudioWave,
                "ogg" => AudioOgg,
                "mp4" => VideoMp4,
                _ => ApplicationOctetStream
            };
        }
		private static MIME _textPlain;
		public static MIME TextPlain => _textPlain ??= new MIME("text/plain");
        private static MIME _textPlainUTF8;
		public static MIME TextPlainUTF8 => _textPlainUTF8 ??= new MIME("text/plain; charset=utf-8");
        private static MIME _textHtml;
		public static MIME TextHtml => _textHtml ??= new MIME("text/html");
        private static MIME _textCss;
		public static MIME TextCss => _textCss ??= new MIME("text/css");
        private static MIME _imageJpeg;
		public static MIME ImageJpeg => _imageJpeg ??= new MIME("image/jpeg");
        private static MIME _imagePng;
		public static MIME ImagePng => _imagePng ??= new MIME("image/png");
        private static MIME _imageGif;
		public static MIME ImageGif => _imageGif ??= new MIME("image/gif");
        private static MIME _imageSvgXml;
		public static MIME ImageSvgXml => _imageSvgXml ??= new MIME("image/svg+xml");
        private static MIME _audioWave;
		public static MIME AudioWave => _audioWave ??= new MIME("audio/wave");
        private static MIME _audioWav;
		public static MIME AudioWav => _audioWav ??= new MIME("audio/wav");
        private static MIME _audioXWav;
		public static MIME AudioXWav => _audioXWav ??= new MIME("audio/x-wav");
        private static MIME _audioXPnWav;
		public static MIME AudioXPnWav => _audioXPnWav ??= new MIME("audio/x-pn-wav");
        private static MIME _audioMpeg;
		public static MIME AudioMpeg => _audioMpeg ??= new MIME("audio/mpeg");
        private static MIME _audioOgg;
		public static MIME AudioOgg => _audioOgg ??= new MIME("audio/ogg");
        private static MIME _audioWebm;
		public static MIME AudioWebm => _audioWebm ??= new MIME("audio/webm");
        private static MIME _videoWebm;
		public static MIME VideoWebm => _videoWebm ??= new MIME("video/webm");
        private static MIME _videoMp4;
		public static MIME VideoMp4 => _videoMp4 ??= new MIME("video/mp4");
        private static MIME _videoOgg;
		public static MIME VideoOgg => _videoOgg ??= new MIME("video/ogg");
        private static MIME _applicationJson;
		public static MIME ApplicationJson => _applicationJson ??= new MIME("application/json");
        private static MIME _applicationJavascript;
		public static MIME ApplicationJavascript => _applicationJavascript ??= new MIME("application/javascript");
        private static MIME _applicationEcmascript;
		public static MIME ApplicationEcmascript => _applicationEcmascript ??= new MIME("application/ecmascript");
        private static MIME _applicationOctetStream;
		public static MIME ApplicationOctetStream => _applicationOctetStream ??= new MIME("application/octet-stream");
        private static MIME _multipartFormData;
		public static MIME MultipartFormData => _multipartFormData ??= new MIME("multipart/form-data");
        private static MIME _multipartByteranges;
		public static MIME MultipartByteranges => _multipartByteranges ??= new MIME("multipart/byteranges");
    }
}
