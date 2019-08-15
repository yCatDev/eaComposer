using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FFMpegCore;
using FFMpegCore.FFMPEG;

namespace EasyAnimComposer {
    class Program {

        public static string name, path;
        public static List<string> img_paths;
        public static int fps = 15;
        public static bool ConsoleMode = false;

        static void Main (string[] args) {
            Console.WriteLine ("Welcome to EASY ANIMATION COMPOSER");            
            if (args.Length>0)
                ConsoleMode = true;
            if (ConsoleMode)
                RunConsole(args);            
            else {
                Console.WriteLine ("You can also run program with arguments: eacomposer [name] [path-to-dir-with-images] [FPS]");
                args = new string[3];
                Console.WriteLine("Write project name: ");
                args[0] = Console.ReadLine();
                Console.WriteLine("Write directory name: ");
                args[1] = Console.ReadLine();
                Console.WriteLine("Write FPS (15 by default): ");
                string tmp = Console.ReadLine();
                if (string.IsNullOrEmpty(tmp))
                    args[2] = "15";
                else 
                    args[2] = tmp;

                RunConsole(args);
            }
            fps += fps / 10;
            FFMpegOptions.Configure (new FFMpegOptions { RootDirectory = AppContext.BaseDirectory + @"\bin" });
            Console.WriteLine ($"Working on project {name}");
            Console.WriteLine ($"Step 1: Add images ({img_paths.Capacity})");
            ImageInfo[] frames = new ImageInfo[img_paths.Capacity];
            for (int i = 0; i < img_paths.Capacity; i++) {
                Console.Write('.');
                frames[i] = new ImageInfo (img_paths[i]);
            }

            path += "/"+name+".mp4";
            if (File.Exists (path))
                new FileInfo (path).Delete ();
            Console.WriteLine ();
            Console.WriteLine ($"Step 2: Creating video on path {path}");
            
            FFMpeg encoder = new FFMpeg ();
            encoder.JoinImageSequence (new FileInfo (path), fps, frames);
            
            Console.WriteLine ($"Finished!");
            encoder.Dispose();
        }

        private static string SortPaths (string inVal) {
            if (int.TryParse (Path.GetFileNameWithoutExtension (inVal), out int o)) {                
                return string.Format ("{0:0000000000}", o);
            } else
                return inVal;
        }

        public static void RunGTK()
        {

        }

        public static void RunConsole(string[] args)
        {
            
            if (args.Length < 2) {
                
                Console.ReadLine ();
                return;
            }

            name = args[0];
            path = AppContext.BaseDirectory + args[1];
            img_paths = new List<string> ();
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

            if (!int.TryParse (args[2], out int fps)) {
                Console.WriteLine ($"Error: FPS must containts numbers only");
                Console.ReadLine ();
                return;
            }
        }

    }
}