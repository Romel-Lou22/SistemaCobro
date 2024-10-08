using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace SistemaCobro
{
    public partial class Cedula : Form
    {
        // Obtén la cadena de conexión del archivo de configuración
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;

        public Cedula()
        {
            InitializeComponent();
        }

        private void btnValidar_Click(object sender, EventArgs e)
        {
            // Obtener la cédula ingresada por el usuario
            string ncedul = txtCedula.Text;

            // Verificar si el campo de cédula está vacío
            if (string.IsNullOrWhiteSpace(ncedul))
            {
                MessageBox.Show("Por favor, ingrese una cédula válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Llamar al procedimiento almacenado para validar la cédula
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Crear el comando para ejecutar el procedimiento almacenado
                    using (SqlCommand command = new SqlCommand("ValidarCedula", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro de entrada
                        command.Parameters.AddWithValue("@ncedula", ncedul);

                        // Ejecutar el procedimiento almacenado
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Leer el resultado del procedimiento y mostrarlo en un MessageBox
                                    string mensaje = reader["Resp"].ToString();
                                    MessageBox.Show(mensaje, "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Manejar errores de conexión o ejecución
                    MessageBox.Show("Ocurrió un error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
