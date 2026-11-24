using System;
using System.Drawing;
using System.Windows.Forms;

namespace TiendaApp
{
    /// <summary>Helpers para construir pantallas por codigo y mostrar mensajes.</summary>
    internal static class UI
    {
        public static Label Lbl(string texto, int x, int y)
        {
            return new Label { Text = texto, Left = x, Top = y + 3, AutoSize = true };
        }

        public static TextBox Txt(int x, int y, int ancho, int maxLength = 0)
        {
            TextBox t = new TextBox { Left = x, Top = y, Width = ancho };
            if (maxLength > 0) t.MaxLength = maxLength;
            return t;
        }

        public static Button Btn(string texto, int x, int y, int ancho, EventHandler click)
        {
            Button b = new Button
            {
                Text = texto,
                Left = x,
                Top = y,
                Width = ancho,
                Height = 28,
                UseVisualStyleBackColor = true
            };
            b.Click += click;
            return b;
        }

        public static DataGridView Grid(int x, int y, int ancho, int alto)
        {
            return new DataGridView
            {
                Left = x,
                Top = y,
                Width = ancho,
                Height = alto,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
        }

        // Defensa extra: aunque el menu ya oculta estos modulos a los cajeros, el propio formulario se cierra si no es Administrador.
        public static void SoloAdministrador(Form f)
        {
            f.Load += delegate
            {
                if (!Modelo.Sesion.EsAdministrador)
                {
                    Aviso("Acceso restringido: solo el Administrador puede usar este módulo.");
                    f.Close();
                }
            };
        }

        public static string Dinero(decimal valor)
        {
            return valor.ToString("N2");
        }

        public static void Info(string mensaje)
        {
            MessageBox.Show(mensaje, "Informacion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Atencion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool Confirmar(string mensaje)
        {
            return MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }
    }
}
