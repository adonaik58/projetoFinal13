import API from "../API/API.js";

// Declare variables
const openWindowCreateUser = document.querySelector(".table-ticket .head-table .more-field button");
const closeWindowCreateUser = document.querySelector(".window-create-user .painel .head-painel button");
const backSidebar = document.querySelector(".window-create-user");
const menuContent = document.querySelector(".window-create-user .painel");
const Form = document.querySelector(".window-create-user .body-painel form");
const title = document.querySelector(".body-painel > h1");
const boxNewPassword = document.querySelector(".new_password");

// getData();
getData();

// Edit user

async function getData() {
  const table = document.querySelector("table tbody");

  const userData = await API.userService.getUsers();

  console.log(userData);

  var content = "";
  if (userData) {
    userData.map(
      (h, i) =>
        (content += `
      <tr>
        <td>${+i + 1}</td>
        <td>
          <i class='fas fa-circle' style='color: ${h.active ? "#00f178" : "#ccc"}'></i>
        </td>
        <td>${h.code == "2" ? "Admin" : "Operador"}</td>
        <td>${h.nome_completo}</td>
        <td>${h.nome}</td>
        <td>
          <button type="button" class="edit-user" value="${h.id}"><i class="fas fa-pen"></i></button>
            <button type="button" class="delete-user" value="${h.id}"><i class="fas fa-trash"></i></button>
        </td>
      <tr>
    `)
    );
    table.innerHTML = content;
  }

  // return content.length > 0 ? true : false;
}

// Show window to create new user

openWindowCreateUser.onclick = () => {
  const fullname = document.querySelector("input[name='fullName']");
  const name = document.querySelector("input[name='name']");
  const senha = document.querySelector("input[name='new_password']");
  const selectProfile = document.querySelector("select");

  fullname.value = "";
  name.value = "";
  senha.value = "";
  selectProfile.value = "";
  showForm();
  Form.removeAttribute("data-id");
  title.textContent = "Criar novo utilizador";
  boxNewPassword.style.display = "none";
  boxNewPassword.nextElementSibling.querySelector("label").textContent = "Senha";
};
closeWindowCreateUser.onclick = (e) => {
  if (e.target.tagName === "BUTTON" || e.target.tagName === "I") {
    menuContent.classList.remove("active");
    backSidebar.classList.remove("fadein");
    setTimeout(() => {
      backSidebar.classList.remove("active");
    }, 300);
  }
};

const onSubmitForm = Form.querySelector("button");

onSubmitForm.onclick = async () => {
  const fullname = document.querySelector("input[name='fullName']");
  const name = document.querySelector("input[name='name']");
  const senha = document.querySelector("input[name='new_password']");
  const confirmarSenha = document.querySelector("input[name='passwordConfirme']");
  const selectProfile = document.querySelector("select");

  let formData = new FormData(Form);

  const formID = Form.dataset.id;

  if (!formID) {
    const response = await API.userService.newUser(formData);
    FNtoast(response);
    if (response.status) {
      fullname.value = "";
      name.value = "";
      senha.value = "";
      selectProfile.value = "";

      getData();
    }
  } else {
    const response = await API.userService.updateUser(formData, formID);

    FNtoast(response);
    if (response.status) {
      senha.value = "";
      confirmarSenha.value = "";

      getData();
    }
  }
};

setInterval(() => {
  const button = document.querySelectorAll("tr button.edit-user");
  button.forEach((edit) => {
    edit.onclick = async () => {
      title.textContent = "Atualizar utilizador";
      Form.setAttribute("data-id", edit.value);
      const response = await API.userService.getUserById(edit.value);
      if (!response.status) {
        boxNewPassword.style.display = "grid";
        boxNewPassword.nextElementSibling.querySelector("label").textContent = "Senha de confirmação";
        console.log(boxNewPassword);
        showForm();
        const data = response[0];

        const fullname = document.querySelector("input[name='fullName']");
        const name = document.querySelector("input[name='name']");
        const selectProfile = document.querySelector("select");

        fullname.value = data.nome_completo;
        name.value = data.nome;
        selectProfile.value = +data.code;
      }
    };
  });
}, 500);

function showForm() {
  backSidebar.classList.add("active");
  backSidebar.classList.add("fadein");
  setTimeout(() => {
    menuContent.classList.add("active");
  }, 100);
}

//Toast message

function FNtoast(response) {
  const toast = document.querySelector(".drt .toast");
  const icon = toast.querySelector(".icon i");

  toast.classList.add(response.status ? "success" : "error");
  icon.classList.add(response.status ? "fa-check" : "fa-times");

  toast.querySelector(".text p").textContent = response.message;
  setTimeout(() => {
    icon.classList.remove(response.status ? "fa-check" : "fa-times");
    toast.classList.remove(response.status ? "success" : "error");
    console.log(toast.classList);
  }, 6000);
}
