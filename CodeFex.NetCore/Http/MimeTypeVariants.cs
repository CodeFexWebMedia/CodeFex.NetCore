using System;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace CodeFex.NetCore.Http
{
    public class MimeTypeVariants
    {
        public string[] Variants { get; set; }

        public string MainVariant
        {
            get
            {
                return Variants[Variants.Length - 1];
            }
        }
        public MimeTypeVariants(string mimetype) : this(mimetype, null)
        {
        }

        public MimeTypeVariants(string mimetype, string[] variants)
        {
            if (mimetype == null) throw new ArgumentNullException(nameof(mimetype));

            if (!MediaTypeHeaderValue.TryParse(mimetype, out var mediaTypeHeaderValue))
            {
                throw new ArgumentException("Invalid mimeType syntax");
            }

            var mimeTypeValues = mediaTypeHeaderValue.MediaType.Split('/');

            if (mimeTypeValues.Length != 2) throw new ArgumentException(nameof(mimetype));

            List<string> variantsCollection = null;
            if (variants != null)
            {
                variantsCollection = new List<string>(variants);
            }
            variantsCollection = variantsCollection ?? new List<string>();
            variantsCollection.AddRange(mimeTypeValues[1].Split('+'));

            Variants = variantsCollection.ToArray();
        }

        public bool IsVariantOf(string mediaType)
        {
            if (mediaType == null) return false;

            var mimeTypeValues = mediaType.Split('/');
            if (mimeTypeValues.Length != 2) return false;

            var variants = mimeTypeValues[1].Split('+');

            return MainVariant.Equals(variants[variants.Length-1], StringComparison.OrdinalIgnoreCase);
        }
    }
}
