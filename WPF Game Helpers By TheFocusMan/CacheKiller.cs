using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfGame
{
    public static class CacheKiller
    {
        internal static List<BitmapSource> ImagesToDestroy = new List<BitmapSource>();

        /// <summary>
        /// Killin the <see cref="ImageCache"/> data
        /// Warnig: Dont use it while having pictures on the screen
        /// </summary>
        /// <exception cref="StackOverflowException"></exception>
        public static void AggresseveDispose()
        {
            // זיון רציני
            var type = typeof(BitmapImage).Assembly.GetType("System.Windows.Media.Imaging.ImagingCache"); // kill the chase (לזיין את זה עד הסוף)
            var imagesfield = type.GetField("_imageCache", DefualtFuckStatic); // להרוג את זה במיידי
            var decodersfield = type.GetField("_decoderCache", DefualtFuckStatic); // להרוג את זה במיידי
            // kill the memery to 0
            Hashtable images = imagesfield.GetValue(null) as Hashtable;
            Hashtable decoders = decodersfield.GetValue(null) as Hashtable;
            foreach (DictionaryEntry image in images) // לזיין את המפנחים
            {
                DestroyObject((image.Value as WeakReference).Target);
                //KillBitmap((image.Value as WeakReference).Target as BitmapImage);
                //GC.SuppressFinalize(image.Value);
            }
            foreach (DictionaryEntry decoding in decoders)
            {
                DestroyObject((decoding.Value as WeakReference).Target);
                //KillDecoder((decoding.Value as WeakReference).Target as BitmapDecoder);
                //GC.SuppressFinalize(decoding.Value);
            }
            decoders.Clear();
            images.Clear();

            typeof(BitmapImage).Assembly.GetType("MS.Internal.FontCache.BufferCache").GetMethod("Reset", DefualtFuckStatic).Invoke(null, null);
        }


        internal static Hashtable GetImageChache()
        {
            var type = typeof(BitmapImage).Assembly.GetType("System.Windows.Media.Imaging.ImagingCache"); // kill the chase (לזיין את זה עד הסוף)
            var imagesfield = type.GetField("_imageCache", DefualtFuckStatic); // להרוג את זה במיידי
            // kill the memery to 0
            Hashtable images = imagesfield.GetValue(null) as Hashtable;
            return images;
        }

        private static List<object> storedvalues = new List<object>();

        /// <summary>
        /// Unstable Fuction for dispose wpf objects
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyObject(object obj)
        {
            DestroyObjectPrivate(obj);
            storedvalues.Clear();
        }

        /// <summary>
        /// Unstable Fuction for dispose wpf objects this function is better than destroy object
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyImage(BitmapSource image)
        {
            if (ImageCanDispose(image))
            {
                DestroyObject(image);
            }
        }

        private static bool ImageCanDispose(BitmapSource image)
        {
            if (image is BitmapImage)
            {
                var cache = GetImageChache();
                return !cache.ContainsKey((image as BitmapImage).UriSource);
                /*var source = image.GetType().GetProperty("WicSourceHandle", DefualtFuck);
                var duck = image.GetType().GetField("_convertedDUCEPtr", DefualtFuck); // מציאת אובייקטים והשוואה
                var vals = cache.Values.OfType<WeakReference>()
                    .Where(x => x.IsAlive)
                    .Select(x => (source.GetValue(x.Target) as SafeHandle).DangerousGetHandle());
                var vlas1 = cache.Values.OfType<WeakReference>()
                    .Where(x => x.IsAlive)
                    .Select(x =>
                    {
                        var handle = duck.GetValue(x.Target) as SafeHandle;
                        var ptr = (handle)?.DangerousGetHandle();
                        handle?.SetHandleAsInvalid();
                        return ptr;
                    });
                var varls = (source.GetValue(image) as SafeHandle).DangerousGetHandle();
                var varls1 = (duck.GetValue(image) as SafeHandle).DangerousGetHandle();
                return !(vals.Contains(varls) && vlas1.Contains(varls1));*/
            }
            return true;
        }

        [SecurityCritical]
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        [SecurityCritical]
        private static void DestroyObjectPrivate(object obj)
        {
            if (storedvalues.Contains(obj) || obj == null) return;
            else storedvalues.Add(obj);
            if (obj is IEnumerable enumable)
            {
                foreach (var v in enumable)
                    DestroyObjectPrivate(v);
            }
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
            if (obj is IList enumable1)
            {
                if (!enumable1.IsReadOnly)
                    enumable1.Clear();
            }
            if (obj is IDictionary enumable2)
                enumable2.Clear();
            if (obj is IntPtr)
                DeleteObject((IntPtr)obj);
            if (Marshal.IsComObject(obj))
                Marshal.ReleaseComObject(obj);
            if (obj is Freezable freezable)
                freezable.Freeze();
            var type = obj.GetType();
#if false
            var names = new string[]
            {
                "System.Windows.Media.Composition.DUCE+Map",
                "System.Windows.Media.Composition.DUCE+MultiChannelResource",
                "System.Windows.Media.Composition.DUCE+Map+Entry"
            };
#else
            var names = new string[0];
#endif
            //if ((obj is DependencyObject || names.Contains(type.FullName) || obj is WeakReference) && obj is not FrameworkElement) // זיוןן גרסה 2
            //{
                var valus = obj.GetType()
                    .GetFields(DefualtFuck)
                    .Select(x => x.GetValue(obj))
                    .ToArray(); // להשיג את כל המשתנים ישירות
                foreach (var v in valus)
                    DestroyObjectPrivate(v);
            //}
        }

        public const BindingFlags DefualtFuck = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic;
        public const BindingFlags DefualtFuckStatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.NonPublic;
    }
}
