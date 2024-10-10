using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace SistemaCobro
{
    public partial class NuevoUsuario : Form
    {
        //private string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;
        // Cadena de conexión a tu base de datos
        private string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;


        public NuevoUsuario()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validar que los campos no estén vacíos
            if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtCedula.Text) ||
                string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtSector.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Capturar los valores de los TextBox
            long codigoUsuario;
            if (!long.TryParse(txtCodigo.Text, out codigoUsuario))
            {
                MessageBox.Show("El código debe ser numérico.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cedula = txtCedula.Text;
            string nombres = txtNombres.Text;
            string sector = txtSector.Text;

            // Consulta SQL utilizando parámetros
            string query = "INSERT INTO Usuarios (CodigoUsuario, Cedula, UsuarioSistema, LugarSistema) " +
                           "VALUES (@codigoUsuario, @cedula, @nombres, @sector)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Asignar valores a los parámetros
                        cmd.Parameters.AddWithValue("@codigoUsuario", codigoUsuario);
                        cmd.Parameters.AddWithValue("@cedula", cedula);
                        cmd.Parameters.AddWithValue("@nombres", nombres);
                        cmd.Parameters.AddWithValue("@sector", sector);

                        // Ejecutar la consulta
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Usuario registrado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo registrar el usuario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Error SQL al conectar a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al conectar a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }


        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtCedula.Clear();
            txtCodigo.Clear();
            txtNombres.Clear();
            txtSector.Clear();
        }
    }
}
