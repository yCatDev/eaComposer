using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.FFMPEG;
using PSDFile;

namespace EasyAnimComposer {
    class Program {

        static void Main (string[] args) {
            Console.WriteLine ("Welcome to EASY ANIMATION COMPOSER");
            if (args.Length < 2) {
                Console.WriteLine ("Run program with arguments: eacomposer [name] [path-to-dir-with-images or PSD] [FPS]");
                Console.ReadLine ();
                return;
            }
            string name = args[0];
            string path = AppContext.BaseDirectory + args[1];
            List<string> img_paths = new List<string> ();
            Console.WriteLine ($"Working on project {name}");
            if (!int.TryParse (args[2], out int fps)) {
                Console.WriteLine ($"Error: FPS must containts numbers only");
                Console.ReadLine ();
                return;
            }

            if (!args[1].Contains ('.')) {
                if (!Directory.Exists (path) || Directory.GetFiles (path).Length == 0) {
                    Console.WriteLine ($"Error: Directory on {path} is empty or not exists");
                    Console.ReadLine ();
                    return;
                } else {
                    if (Directory.GetFiles (path, "*.jpg").Length == 0) {
                        if (Directory.GetFiles (path, "*.png").Length == 0) {
                            Console.WriteLine ($"Error: Images in directory {path} not found. Make sure you export files in png or jpg format");
                            Console.ReadLine ();
                            return;
                        } else img_paths = new List<string> (Directory.GetFiles (path, "*.png").OrderBy (SortPaths));
                    } else img_paths = new List<string> (Directory.GetFiles (path, "*.jpg").OrderBy (SortPaths));
                }

            } else {
                PsdFile psd = new PsdFile (path, new LoadContext ());
                Console.WriteLine ($"Step 1: Add images ({psd.Layers.Capacity})");
                Directory.CreateDirectory ("_tmp");
                foreach (Layer l in psd.Layers) {
                    string tmp = "./_tmp/" + l.Name;
                    l.GetBitmap ().Save (tmp);
                    img_paths.Add (tmp);
                }
            }

            fps += fps / 10;
            FFMpegOptions.Configure (new FFMpegOptions { RootDirectory = AppContext.BaseDirectory + @"\bin" });
            Console.WriteLine ($"Step 1: Add images ({img_paths.Capacity})");
            ImageInfo[] frames = new ImageInfo[img_paths.Capacity];
            for (int i = 0; i < img_paths.Capacity; i++) {
                Console.WriteLine (img_paths[i] + " ");
                frames[i] = new ImageInfo (img_paths[i]);
            }

            /*
            var gif = AnimatedGif.AnimatedGif.Create($"{path}{name}.gif",fps+(fps/10));
            foreach (string frame in frames)
            {
                Console.Write('.');
                gif.AddFrame(frame, delay: -1, quality: GifQuality.Bit8);                
            }

            gif.Dispose();*/
            path = path + name + ".mp4";
            if (File.Exists (path))
                new FileInfo (path).Delete ();
            Console.WriteLine ();
            Console.WriteLine ($"Step 2: Creating video on path {path}");
            FFMpeg encoder = new FFMpeg ();

            encoder.JoinImageSequence (new FileInfo (path), fps, frames);
            Console.WriteLine ($"Finished!");
        }

        private static string SortPaths (string inVal) {
            if (int.TryParse (Path.GetFileNameWithoutExtension (inVal), out int o)) {
                Console.WriteLine (string.Format ("{0:0000000000}", o));
                return string.Format ("{0:0000000000}", o);
            } else
                return inVal;
        }
    }
}