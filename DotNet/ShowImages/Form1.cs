﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowImages {
    public partial class Form1 : Form {
        public Form1() {

            InitializeComponent();
            CheckForIllegalCrossThreadCalls = true;
        }

        private void button0_Click(object sender, EventArgs e) {
            PictureBox[] boxes = { pictureBox1, pictureBox2 };
            string[] names = { name1.Text, name2.Text };

            Console.WriteLine("UI thread is {0}",
                  Thread.CurrentThread.ManagedThreadId);
            int pos=0;
            foreach(string name in names) {
                Model.GetFromFileAsync(names[pos]).ContinueWith((t, o) => {
                     
                    Console.WriteLine("Continuation thread is {0}",
                            Thread.CurrentThread.ManagedThreadId);
                    int p = (int)o;
                    boxes[p].Image = t.Result;
                }, pos, TaskScheduler.FromCurrentSynchronizationContext());
                pos++;
            }
           
        }

        private void button2_Click(object sender, EventArgs e) {
            Task<Image> t1 = Model.GetFromFileAsync(name1.Text);
            Console.WriteLine("UI thread is {0}",
                    Thread.CurrentThread.ManagedThreadId);
            Task<Image> t2 = Model.GetFromFileAsync(name2.Text);
         
            Task.Factory.ContinueWhenAll(new Task<Image>[] { t1, t2 }, t => {

                Console.WriteLine("WhenAll continuation in thread {0}",
                    Thread.CurrentThread.ManagedThreadId);
                
                pictureBox1.Image = t[0].Result;
                pictureBox2.Image = t[1].Result;
               
           
            }, CancellationToken.None, 
            TaskContinuationOptions.None, 
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static Task WhenAll(Task[] tasks) {
            int count = tasks.Length;
            var tcs = new TaskCompletionSource<object>();

            foreach(Task t in tasks) {
                t.ContinueWith(_ => {
                    if (Interlocked.Decrement(ref count) == 0)
                        tcs.SetResult(null);
                });
            }
            return tcs.Task;
        }

        private void button1_Click(object sender, EventArgs e) {
            Task<Image> t1 = Model.GetFromFileAsync(name1.Text);
            Console.WriteLine("New Handler UI thread is {0}",
                    Thread.CurrentThread.ManagedThreadId);
            Task<Image> t2 = Model.GetFromFileAsync(name2.Text);
            var ctx = SynchronizationContext.Current;

            WhenAll(new Task<Image>[] { t1, t2 }).ContinueWith(_ => {
                Console.WriteLine("WhenAll continuation in thread {0}",
                    Thread.CurrentThread.ManagedThreadId);
                ctx.Post(s => {
                    Console.WriteLine("ctx delegate in thread {0}",
                   Thread.CurrentThread.ManagedThreadId);
                    pictureBox1.Image = t1.Result;
                    pictureBox2.Image = t2.Result;
                },null);
                
            });
            
            
        }
    }
}
