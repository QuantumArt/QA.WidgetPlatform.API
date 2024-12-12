# Докер-образ WP.API

## Назначение

Образ содержит приложение WP.API, которое входит в состав модулей **QP8.WidgetPlatform.React** и **QP8.WidgetPlatform.Angular**. Использование образа описано в [руководстве пользователя QP8.WidgetPlatform.React](https://storage.qp.qsupport.ru/qa_official_site/images/downloads/qp8-widgets-react_user_man.pdf) и в [руководстве пользователя QP8.WidgetPlatform.Angular](https://storage.qp.qsupport.ru/qa_official_site/images/downloads/qp8-widgets-angular-user-man.pdf)  (в разделе **Руководство по установке**).

## Репозитории

* [DockerHub](https://hub.docker.com/r/qpcms/widget-platform-api/tags): `qpcms/widget-platform-api`
* QA Harbor: `registry.quantumart.ru/qp8-widgets/api`

## История тегов (версий)

### 1.2.0.0

* Обновление до .NET8
* Обновление nuget-пакета QA.DotNetCore.Engine.QpData.Configuration до версии 3.4.1

### 1.1.2.0

* Добавлена возможность включения\выключения Swagger через опцию UseSwagger (#175205)
* Добавлена возможность возврата незаполненных полей для метода `node` (#175018)
* Обновлен nuget-пакет QA.DotNetCore.Engine.QpData.Configuration до версии 3.2.24 (#175018)
* Обновлены nuget-пакеты (#174540)

### 1.1.1.0

* Обновлены nuget-пакеты (#174238)
* Добавлен возврат признака опубликованности для виджетов, добавлен выбор полей для метода `widgets` (#174155)

### 1.1.0.0

* Доработки по таргетированию (#172727)

### 1.0.1.0

* Добавлен фильтр таргетирования по регионам (и настройка)

### 0.5.0.98

* Актуализирована библиотека WP
* Исправлена проблема с пустыми M2M (возвращался link_id)
* Исправлена проблема, что не возвращается SortOrder для виджетов

### 0.5.0.62835

* Базовая версия
