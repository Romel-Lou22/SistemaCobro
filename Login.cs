using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SistemaCobro
{
    public partial class Login : Form
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["miConexion"].ConnectionString;

        public Login()
        {
            InitializeComponent();
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            // Placeholder inicial
            txtUsuario.Text = "Ingrese usuario";
            txtUsuario.ForeColor = System.Drawing.Color.Gray;

            txtPasword.Text = "Ingrese contraseña";
            txtPasword.ForeColor = System.Drawing.Color.Gray;
            txtPasword.PasswordChar = '\0';  // Mostrar texto inicialmente

            txtPasword.MaxLength = 20;
            this.AcceptButton = btnIngresar; // Permite ingresar presionando Enter

            // Asignar eventos para txtUsuario
            txtUsuario.Enter += new EventHandler(txtUsuario_Enter);
            txtUsuario.Leave += new EventHandler(txtUsuario_Leave);

            // Asignar eventos para txtPasword
            txtPasword.Enter += new EventHandler(txtPasword_Enter);
            txtPasword.Leave += new EventHandler(txtPasword_Leave);
        }

        // Evento Enter para txtUsuario
        private void txtUsuario_Enter(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "Ingrese usuario")
            {
                txtUsuario.Text = "";
                txtUsuario.ForeColor = System.Drawing.Color.Black;
            }
        }

        // Evento Leave para txtUsuario
        private void txtUsuario_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtUsuario.Text = "Ingrese usuario";
                txtUsuario.ForeColor = System.Drawing.Color.Gray;
            }
        }

        // Evento Enter para txtPasword
        private void txtPasword_Enter(object sender, EventArgs e)
        {
            if (txtPasword.Text == "Ingrese contraseña")
            {
                txtPasword.Text = "";
                txtPasword.ForeColor = System.Drawing.Color.Black;
                txtPasword.PasswordChar = '•';  // Activa el ocultamiento del texto
            }
        }

        // Evento Leave para txtPasword
        private void txtPasword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPasword.Text))
            {
                txtPasword.PasswordChar = '\0';  // Desactiva el ocultamiento del texto para mostrar el placeholder
                txtPasword.Text = "Ingrese contraseña";
                txtPasword.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (ValidarEntradas())
            {
                if (VerificarCredenciales(txtUsuario.Text.Trim(), txtPasword.Text.Trim()))
                {
                    AbrirFormularioPrincipal();
                }
                else
                {
                    MostrarMensajeError("Usuario o contraseña incorrecta");
                    LimpiarCampos();
                }
            }
        }

        private bool ValidarEntradas()
        {
            // Asegurarse de que el placeholder no cuenta como entrada válida
            if (txtUsuario.Text == "Ingrese usuario" || txtPasword.Text == "Ingrese contraseña" ||
                string.IsNullOrEmpty(txtUsuario.Text.Trim()) || string.IsNullOrEmpty(txtPasword.Text.Trim()))
            {
                MessageBox.Show("Ingrese Usuario y Contraseña", "Datos requeridos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool VerificarCredenciales(string usuario, string contrasena)
        {
            string query = "SELECT COUNT(*) FROM InicioSesion WHERE usuario=@usuario AND contrasena=@contrasena";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario", usuario);
                        cmd.Parameters.AddWithValue("@contrasena", contrasena);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
                catch (SqlException ex)
                {
                    MostrarMensajeError($"Error al conectarse con la base de datos: {ex.Message}");
                    return false;
                }
            }
        }

        private void AbrirFormularioPrincipal()
        {
            MessageBox.Show("Inicio de Sesión Exitoso", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            new Form1().Show();
            this.Hide();
        }

        private void MostrarMensajeError(string mensaje)
        {
            MessageBox.Show(mensaje, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void LimpiarCampos()
        {
            txtUsuario.Clear();
            txtPasword.Clear();
            txtUsuario.Focus();

            // Restaurar los placeholders
            txtUsuario.Text = "Ingrese usuario";
            txtUsuario.ForeColor = System.Drawing.Color.Gray;

            txtPasword.Text = "Ingrese contraseña";
            txtPasword.ForeColor = System.Drawing.Color.Gray;
            txtPasword.PasswordChar = '\0';  // Mostrar texto como placeholder
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
