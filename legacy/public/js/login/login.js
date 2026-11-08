import { API_BASE } from "../utils/baseAPI.js";

const form = document.querySelector("form");
const formName = document.querySelector("form input[type='text']");
const formPassword = document.querySelector("form input[type='password']");
const button = document.querySelector("button[type='button']");

form.onsubmit = (e) => {
  e.preventDefault();
};
button.onclick = async () => {
  const dataForm = new FormData(form);

  const response = await fetch(`${API_BASE}/login/request`, {
    method: "POST",
    body: dataForm,
  })
    .then((response) => response.json())
    .catch((e) => console.log(e));
  if (response.status) {
    const getYear = parseInt(new Date().getFullYear() + 1);
    const getMonth = parseInt(new Date().getMonth());
    const getDay = parseInt(new Date().getDay());

    document.cookie = `token=${response.token}; expires= ${new Date(`${getYear} ${getMonth} ${getDay}`)}`;

    location.href = location.origin + "/";
  } else {
    FNtoast(response);
    // console.log(response.message);
    // alert("Tipo: Erro! \nMessagem: " + response.message);
  }
};

const btnShow = document.querySelector(".see");
const password = document.querySelector(".password");

btnShow.onclick = () => {
  if (password.type === "password") {
    password.type = "text";
    btnShow.textContent = "Ocultar";
  } else {
    password.type = "password";
    btnShow.textContent = "Mostrar";
  }
};

function FNtoast(response) {
  const toast = document.querySelector(".form-content .toast");
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
